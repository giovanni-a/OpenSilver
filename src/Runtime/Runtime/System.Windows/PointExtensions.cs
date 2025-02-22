#if !MIGRATION
using System;
#endif

#if MIGRATION
namespace System.Windows
#else
namespace Windows.Foundation
#endif
{
	internal static class PointExtensions
    {
        public static bool IsClose(this Point @this, Point point)
        {
            return @this.X.IsClose(point.X) && @this.Y.IsClose(point.Y);
        }
    }
}