using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TR2RandomizerView.Model
{
    public static class ProfileString
    {
        /**
         * The profile string is composed of three parts. 
         * 1. Between position 0 and the first position of R, each rando element is encoded as follows:
         *         RandoElementID + SeedIndex + {CustomInt|CustomBool}
         *    No separator is used between these. RandoElementID is L, U, A etc; SeedIndex is the base 36 index in
         *    the seed array to use. If the element has a custom int, this number is added. If it has a custom
         *    bool and that bool is true, b is added.
         * 2. A single R marks the end of the rando elements and the start of the seeds array.
         * 3. Each unique seed is added to the end of the string, separated by R. The seed is changed to base 36
         *    to keep the string as short as possible (26 chars + 10 digits = 36).
         *    We could potentially use punctuation in order to use a higher base, which would shorten the string further.
         */

        private const char _levelSeqID = 'L';
        private const char _unarmedID  = 'U';
        private const char _ammolessID = 'A';
        private const char _rewardsID  = 'B';
        private const char _sunsetsID  = 'S';
        private const char _tracksID   = 'T';
        private const char _itemsID    = 'I';
        private const char _enemiesID  = 'E';
        private const char _secretsID  = 'C';
        private const char _texturesID = 'X';

        private static readonly char[] _allIDs = new char[]
        {
            _levelSeqID, _unarmedID, _ammolessID, _rewardsID, _sunsetsID, _tracksID, _itemsID, _enemiesID, _secretsID, _texturesID
        };

        private const char _seedID     = 'R';
        private const char _boolID     = 'b';

        private const string _chars = "0123456789abcdefghijklmnopqrstuvwxyz";

        private const int _seedBase = 36;
        private const int _intBase = 36;

        public static string Create(ControllerOptions controller)
        {
            List<int> seeds = new List<int>();

            StringBuilder sb = new StringBuilder();
            BuildRandoString(_levelSeqID, controller.RandomizeLevelSequencing, controller.LevelSequencingSeed, seeds, sb);
            BuildRandoString(_unarmedID, controller.RandomizeUnarmedLevels, controller.UnarmedLevelsSeed, seeds, sb);
            if (controller.RandomizeUnarmedLevels)
            {
                sb.Append(ToBase((int)controller.UnarmedLevelCount, _intBase));
            }
            BuildRandoString(_ammolessID, controller.RandomizeAmmolessLevels, controller.AmmolessLevelsSeed, seeds, sb);
            if (controller.RandomizeAmmolessLevels)
            {
                sb.Append(ToBase((int)controller.AmmolessLevelCount, _intBase));
            }

            BuildRandoString(_rewardsID, controller.RandomizeSecretRewards, controller.SecretRewardSeed, seeds, sb);
            BuildRandoString(_sunsetsID, controller.RandomizeSunsets, controller.SunsetsSeed, seeds, sb);
            if (controller.RandomizeSunsets)
            {
                sb.Append(ToBase((int)controller.SunsetCount, _intBase));
            }

            BuildRandoString(_tracksID, controller.RandomizeAudioTracks, controller.AudioTracksSeed, seeds, sb);
            if (controller.RandomizeAudioTracks && controller.RandomAudioTracksIncludeBlank)
            {
                sb.Append(_boolID);
            }

            BuildRandoString(_itemsID, controller.RandomizeItems, controller.ItemSeed, seeds, sb);
            if (controller.RandomizeItems && controller.IncludeKeyItems)
            {
                sb.Append(_boolID);
            }
            BuildRandoString(_enemiesID, controller.RandomizeEnemies, controller.EnemySeed, seeds, sb);
            BuildRandoString(_secretsID, controller.RandomizeSecrets, controller.SecretSeed, seeds, sb);
            if (controller.RandomizeSecrets && controller.HardSecrets)
            {
                sb.Append(_boolID);
            }
            BuildRandoString(_texturesID, controller.RandomizeTextures, controller.TextureSeed, seeds, sb);

            List<string> base36Seeds = new List<string>();
            seeds.ForEach(e => base36Seeds.Add(ToBase(e, _seedBase)));

            return sb.ToString() + _seedID + string.Join(_seedID.ToString(), base36Seeds);
        }

        private static void BuildRandoString(char id, bool randoOption, int seed, List<int> seeds, StringBuilder sb)
        {
            if (randoOption)
            {
                if (!seeds.Contains(seed))
                {
                    seeds.Add(seed);
                }
                
                sb.Append(id).Append(ToBase(seeds.IndexOf(seed), _intBase));
            }
        }

        private static string ToBase(int value, int bse)
        {
            if (value == 0)
            {
                return "0";
            }

            StringBuilder sb = new StringBuilder();
            while (value > 0)
            {
                sb.Append(_chars[value % bse]);
                value /= bse;
            }

            return sb.ToString();
        }

        private static int FromBase(string value, int bse)
        {
            char[] chars = value.ToCharArray();
            int result = 0;
            int pos = 0;
            foreach (char c in chars)
            {
                result += _chars.IndexOf(c) * (int)Math.Pow(bse, pos++);
            }
            return result;
        }

        public static void Apply(ControllerOptions controller, string profile)
        {
            int rpos = profile.IndexOf(_seedID);
            if (rpos == -1)
            {
                throw new ArgumentException(string.Format("Missing {0} separator in profile string.", _seedID));
            }
            if (rpos == 0 || rpos == profile.Length - 1)
            {
                throw new ArgumentException("Invalid separator position in profile string.");
            }

            string randoElements = profile.Substring(0, rpos);
            string seedElements = profile.Substring(rpos + 1); //skip the first seed separator

            List<int> seeds = new List<int>();
            foreach (string seedString in seedElements.Split(_seedID))
            {
                seeds.Add(FromBase(seedString, _seedBase));
            }

            bool randomizeLevelSequencing = false, randomizeUnarmedLevels = false, randomizeAmmolessLevels = false,
                randomizeSecretRewards = false, randomizeSunsets = false, randomizeAudioTracks = false,
                randomizeItems = false, randomizeSecrets = false, randomizeEnemies = false, randomizeTextures = false;

            int levelSequencingSeed = -1, unarmledSeed = -1, ammolessSeed = -1,
                secretRewardsSeed = -1, sunsetsSeed = -1, tracksSeed = -1,
                itemsSeed = -1, secretsSeed = -1, enemiesSeed = -1, texturesSeed = -1;

            int unarmedLevelCount = -1, ammolessLevelCount = -1, sunsetCount = -1;

            bool blankTracks = false, includeKeyItems = false, hardSecrets = false;

            for (int i = 0; i < randoElements.Length; i++)
            {
                char randoID = randoElements[i];
                switch (randoID)
                {
                    case _levelSeqID:
                        randomizeLevelSequencing = true;
                        break;
                    case _unarmedID:
                        randomizeUnarmedLevels = true;
                        break;
                    case _ammolessID:
                        randomizeAmmolessLevels = true;
                        break;
                    case _rewardsID:
                        randomizeSecretRewards = true;
                        break;
                    case _sunsetsID:
                        randomizeSunsets = true;
                        break;
                    case _tracksID:
                        randomizeAudioTracks = true;
                        break;
                    case _itemsID:
                        randomizeItems = true;
                        break;
                    case _enemiesID:
                        randomizeEnemies = true;
                        break;
                    case _secretsID:
                        randomizeSecrets = true;
                        break;
                    case _texturesID:
                        randomizeTextures = true;
                        break;
                    default:
                        continue;
                }

                if (i == randoElements.Length - 1)
                {
                    throw new ArgumentException("Invalid profile string - unexpected EOF.");
                }

                string seedString = randoElements[++i].ToString();
                int seedIndex = FromBase(seedString, _intBase);
                if (seedIndex < 0 || seedIndex > seeds.Count - 1)
                {
                    throw new ArgumentException(string.Format("Invalid seed index ({0}).", seedString));
                }

                switch (randoID)
                {
                    case _levelSeqID:
                        levelSequencingSeed = seeds[seedIndex];
                        break;
                    case _unarmedID:
                        unarmledSeed = seeds[seedIndex];
                        break;
                    case _ammolessID:
                        ammolessSeed = seeds[seedIndex];
                        break;
                    case _rewardsID:
                        secretRewardsSeed = seeds[seedIndex];
                        break;
                    case _sunsetsID:
                        sunsetsSeed = seeds[seedIndex];
                        break;
                    case _tracksID:
                        tracksSeed = seeds[seedIndex];
                        break;
                    case _itemsID:
                        itemsSeed = seeds[seedIndex];
                        break;
                    case _enemiesID:
                        enemiesSeed = seeds[seedIndex];
                        break;
                    case _secretsID:
                        secretsSeed = seeds[seedIndex];
                        break;
                    case _texturesID:
                        texturesSeed = seeds[seedIndex];
                        break;
                    default:
                        continue;
                }

                if (i < randoElements.Length - 2 && !_allIDs.Contains(randoElements[i + 1]))
                {
                    char c = randoElements[++i];
                    switch (randoID)
                    {
                        case _unarmedID:
                            unarmedLevelCount = FromBase(c.ToString(), _intBase);
                            break;
                        case _ammolessID:
                            ammolessLevelCount = FromBase(c.ToString(), _intBase);
                            break;
                        case _sunsetsID:
                            sunsetCount = FromBase(c.ToString(), _intBase);
                            break;
                        case _tracksID:
                            blankTracks = c == _boolID;
                            break;
                        case _secretsID:
                            hardSecrets = c == _boolID;
                            break;
                        case _itemsID:
                            includeKeyItems = c == _boolID;
                            break;
                    }
                }
            }

            controller.RandomizeLevelSequencing = randomizeLevelSequencing;
            controller.RandomizeUnarmedLevels = randomizeUnarmedLevels;
            controller.RandomizeAmmolessLevels = randomizeAmmolessLevels;
            controller.RandomizeSecretRewards = randomizeSecretRewards;
            controller.RandomizeSunsets = randomizeSunsets;
            controller.RandomizeAudioTracks = randomizeAudioTracks;
            controller.RandomizeItems = randomizeItems;
            controller.RandomizeEnemies = randomizeEnemies;
            controller.RandomizeSecrets = randomizeSecrets;
            controller.RandomizeTextures = randomizeTextures;

            if (levelSequencingSeed != -1) controller.LevelSequencingSeed = levelSequencingSeed;
            if (unarmledSeed != -1) controller.UnarmedLevelsSeed = unarmledSeed;
            if (ammolessSeed != -1) controller.AmmolessLevelsSeed = ammolessSeed;
            if (secretRewardsSeed != -1) controller.SecretRewardSeed = secretRewardsSeed;
            if (sunsetsSeed != -1) controller.SunsetsSeed = sunsetsSeed;
            if (tracksSeed != -1) controller.AudioTracksSeed = tracksSeed;
            if (itemsSeed != -1) controller.ItemSeed = itemsSeed;
            if (enemiesSeed != -1) controller.EnemySeed = enemiesSeed;
            if (secretsSeed != -1) controller.SecretSeed = secretsSeed;
            if (texturesSeed != -1) controller.TextureSeed = texturesSeed;

            if (unarmedLevelCount > 0) controller.UnarmedLevelCount = (uint)unarmedLevelCount;
            if (ammolessLevelCount > 0) controller.AmmolessLevelCount = (uint)ammolessLevelCount;
            if (sunsetCount > 0) controller.SunsetCount = (uint)sunsetCount;

            controller.RandomAudioTracksIncludeBlank = blankTracks;
            controller.HardSecrets = hardSecrets;
            controller.IncludeKeyItems = includeKeyItems;
        }
    }
}