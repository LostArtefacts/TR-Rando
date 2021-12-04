using System;
using System.Collections.Generic;
using System.Linq;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TextureHolder<E, L> : IDisposable
        where E : Enum
        where L : class
    {
        public AbstractTextureMapping<E, L> Mapping { get; private set; }
        public Dictionary<AbstractTextureSource, string> Variants { get; private set; }

        internal TextureHolder(AbstractTextureMapping<E, L> mapping, ITextureVariantHandler handler, TextureHolder<E, L> parentHolder = null)
        {
            Mapping = mapping;
            Variants = new Dictionary<AbstractTextureSource, string>();

            // Check first for any grouped sources, but only if the parent holder is null
            // as regrouping is not currently possible.
            List<StaticTextureSource<E>> handledSources = new List<StaticTextureSource<E>>();
            if (parentHolder == null)
            {
                List<TextureGrouping<E>> groupingList = mapping.StaticGrouping;
                foreach (TextureGrouping<E> staticGrouping in groupingList)
                {
                    // Choose a variant for the leader, then assign this to the followers if they support it
                    string variant = handler.GetSourceVariant(staticGrouping.Leader);
                    Variants.Add(staticGrouping.Leader, variant);
                    handledSources.Add(staticGrouping.Leader);

                    foreach (StaticTextureSource<E> source in staticGrouping.Followers)
                    {
                        if (source.HasVariants)
                        {
                            // Are we enforcing a specific colour for this theme?
                            if (staticGrouping.ThemeAlternatives.ContainsKey(variant) && staticGrouping.ThemeAlternatives[variant].ContainsKey(source))
                            {
                                Variants.Add(source, staticGrouping.ThemeAlternatives[variant][source]);
                            }
                            // Otherwise, does the grouped source have the same variant available?
                            else if (source.Variants.Contains(variant))
                            {
                                Variants.Add(source, variant);
                                // If persistent textures are being used, have outer store what has been assigned to this source.
                                handler.StoreVariant(source, variant);
                            }
                            // Otherwise, just add another random value for now (we ignore single variant sources such as FL/DL Spooky theme)
                            else if (source.Variants.Length > 1)
                            {
                                Variants.Add(source, handler.GetSourceVariant(source));
                            }

                            handledSources.Add(source);
                        }
                    }
                }
            }
            else
            {
                foreach (AbstractTextureSource source in parentHolder.Variants.Keys)
                {
                    Variants[source] = parentHolder.Variants[source];
                }
            }

            foreach (StaticTextureSource<E> source in Mapping.StaticMapping.Keys)
            {
                // Only randomize sources that aren't already grouped and that actually have variants, or if we have a master
                // parent holder, only add it the source if it's not already defined.
                if (source.HasVariants && ((parentHolder == null && !handledSources.Contains(source)) || (parentHolder != null && !Variants.ContainsKey(source))))
                {
                    Variants.Add(source, handler.GetSourceVariant(source));
                }
            }

            // Dynamic changes should be made after static (e.g. for overlays)
            foreach (DynamicTextureSource source in Mapping.DynamicMapping.Keys)
            {
                if (!Variants.ContainsKey(source))
                {
                    Variants.Add(source, handler.GetSourceVariant(source));
                }
            }
        }

        public void Dispose()
        {
            Mapping.Dispose();
        }
    }
}