using TRLevelControl.Model;

namespace TRLevelControlTests.Common;

public partial class TextureTests
{
    private static TRObjectTexture MakeNWClockwiseQuad()
    {
        // 0---1
        //     |
        // 3---2
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(63, 0),
                new(63, 63),
                new(0, 63)
            }
        };
    }

    private static TRObjectTexture MakeNWClockwiseTri1()
    {
        // 0---1
        //     |
        //     2
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(63, 0),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNWClockwiseTri2()
    {
        // 0---1
        //   /   
        // 2
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(63, 0),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNWClockwiseTri3()
    {
        // 0
        //   \   
        // 2---1
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(63, 63),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEClockwiseQuad()
    {
        // 3   0
        // |   |
        // 2---1
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(63, 63),
                new(0, 63),
                new(0, 0),
            }
        };
    }

    private static TRObjectTexture MakeNEClockwiseTri1()
    {
        //     0
        //     |
        // 2---1
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(63, 63),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEClockwiseTri2()
    {
        // 2   0
        //   \ |
        //     1
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(63, 63),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEClockwiseTri3()
    {
        // 2   0
        // | /
        // 1
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(0, 63),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEClockwiseQuad()
    {
        // 2---3
        // |    
        // 1---0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(0, 63),
                new(0, 0),
                new(63, 0),
            }
        };
    }

    private static TRObjectTexture MakeSEClockwiseTri1()
    {
        // 2
        // |    
        // 1---0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(0, 63),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEClockwiseTri2()
    {
        //     2
        //   /    
        // 1---0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(0, 63),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEClockwiseTri3()
    {
        // 1---2
        //   \   
        //     0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(0, 0),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWClockwiseQuad()
    {
        // 1---2
        // |   |
        // 0   3
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(0, 0),
                new(63, 0),
                new(63, 63),
            }
        };
    }

    private static TRObjectTexture MakeSWClockwiseTri1()
    {
        // 1---2
        // |
        // 0
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(0, 0),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWClockwiseTri2()
    {
        // 1
        // | \
        // 0   2
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(0, 0),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWClockwiseTri3()
    {
        //     1
        //   / |
        // 0   2
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(63, 0),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNWAntiClockwiseQuad()
    {
        // 0   3
        // |   |
        // 1---2
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(0, 63),
                new(63, 63),
                new(63, 0),
            }
        };
    }

    private static TRObjectTexture MakeNWAntiClockwiseTri1()
    {
        // 0
        // |
        // 1---2
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(0, 63),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNWAntiClockwiseTri2()
    {
        // 0   2
        // | /
        // 1
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(0, 63),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNWAntiClockwiseTri3()
    {
        // 0   2
        //   \ |
        //     1
        return new()
        {
            Vertices = new()
            {
                new(0, 0),
                new(63, 63),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEAntiClockwiseQuad()
    {
        // 1---0
        // |   
        // 2---3
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(0, 0),
                new(0, 63),
                new(63, 63),
            }
        };
    }

    private static TRObjectTexture MakeNEAntiClockwiseTri1()
    {
        // 1---0
        // |   
        // 2
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(0, 0),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEAntiClockwiseTri2()
    {
        // 1---0
        //   \  
        //     2
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(0, 0),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeNEAntiClockwiseTri3()
    {
        //     0
        //   /  
        // 1---2
        return new()
        {
            Vertices = new()
            {
                new(63, 0),
                new(0, 63),
                new(63, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEAntiClockwiseQuad()
    {
        // 2---1
        // |   |
        // 3   0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(63, 0),
                new(0, 0),
                new(0, 63),
            }
        };
    }

    private static TRObjectTexture MakeSEAntiClockwiseTri1()
    {
        // 2---1
        //     |
        //     0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(63, 0),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEAntiClockwiseTri2()
    {
        //     1
        //   / |
        // 2   0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(63, 0),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSEAntiClockwiseTri3()
    {
        // 1
        // | \
        // 2   0
        return new()
        {
            Vertices = new()
            {
                new(63, 63),
                new(0, 0),
                new(0, 63),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWAntiClockwiseQuad()
    {
        // 3---2
        //     |
        // 0---1
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(63, 63),
                new(63, 0),
                new(0, 0),
            }
        };
    }

    private static TRObjectTexture MakeSWAntiClockwiseTri1()
    {
        //     2
        //     |
        // 0---1
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(63, 63),
                new(63, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWAntiClockwiseTri2()
    {
        // 2
        //   \
        // 0---1
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(63, 63),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }

    private static TRObjectTexture MakeSWAntiClockwiseTri3()
    {
        // 2---1
        //   /
        // 0
        return new()
        {
            Vertices = new()
            {
                new(0, 63),
                new(63, 0),
                new(0, 0),
                new() { U = 0, V = 0 }
            }
        };
    }
}
