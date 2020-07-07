using System;

namespace OpenRGB.NET.Enums
{
    [Flags]
    public enum OpenRGBModeFlags
    {
        HasSpeed = (1 << 0), /* Mode has speed parameter         */
        HasDirectionLR = (1 << 1), /* Mode has left/right parameter    */
        HasDirectionUD = (1 << 2), /* Mode has up/down parameter       */
        HasDirectionHV = (1 << 3), /* Mode has horiz/vert parameter    */
        HasBrightness = (1 << 4), /* Mode has brightness parameter    */
        HasPerLedColor = (1 << 5), /* Mode has per-LED colors          */
        HasModeSpecificColor = (1 << 6), /* Mode has mode specific colors    */
        HasRandomColor = (1 << 7), /* Mode has random color option     */
    }
}
