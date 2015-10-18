using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
   public static class GeoHelper
    {

        public static bool IsPointInside(this BaseLocation location, MainPage.Bounds bound)
        {
            bool isInside = false;
            if (location != null)
            {
                if (bound.East < bound.West)
                {
                    //longitude of the West border is bigger than the easter one
                    if ((-180 <= location.XCoordinate && location.XCoordinate <= bound.East)
                        || (bound.West <= location.XCoordinate && location.XCoordinate <= 180))
                    {
                        if (bound.South <= location.YCoordinate && location.YCoordinate <= bound.North)
                        {
                            isInside = true;
                        }
                    }
                }
                else
                {
                    //longitude of the east border is bigger than the wester one
                    if (bound.West <= location.XCoordinate && location.XCoordinate <= bound.East)
                    {
                        if (bound.South <= location.YCoordinate  && location.YCoordinate <= bound.North)
                        {
                            isInside = true;
                        }
                    }
                }
            }
            return isInside;
        }
    }
}
