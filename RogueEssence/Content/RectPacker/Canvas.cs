using System;

namespace RectPacker
{
    /// <summary>
    /// This type of canvas places rectangles as far to the left as possible (lowest X).
    /// If there is a choice between locations with the same X, it will pick the one with the 
    /// lowest Y.
    /// </summary>
    public class Canvas
    {
        public struct CanvasCell
        {
            public bool occupied;

            public CanvasCell(bool occupied) { this.occupied = occupied; }

            public override string ToString() { return occupied ? "x" : "."; }
        }

        private DynamicTwoDimensionalArray<CanvasCell> _canvasCells =
            new DynamicTwoDimensionalArray<CanvasCell>();

        // Make _canvasCells available to canvas classes derived from this class.
        protected DynamicTwoDimensionalArray<CanvasCell> CanvasCells { get { return _canvasCells; } }

        private int _nbrRectangleAddAttempts = 0;
        public int NbrRectangleAddAttempts { get { return _nbrRectangleAddAttempts; } }

        private int _canvasWidth = 0;
        private int _canvasHeight = 0;

        // Lowest free height deficit found since the last call to SetCanvasDimension
        private int _lowestFreeHeightDeficitSinceLastRedim = Int32.MaxValue;

        private int _nbrCellsGenerated = 0;

        public Canvas()
        {
        }

        /// <summary>
        /// Value denoting an unlimited width or height. You would pass this in to SetCanvasDimensions.
        /// </summary>
        public const int INFINITE_SIZE = short.MaxValue;

        /// <summary>
        /// Sets the dimensions of the canvas.
        /// If there were already rectangles on the canvas when this is called, those rectangles will be removed.
        /// 
        /// Be sure to call this method before you call AddRectangle for the first time.
        /// </summary>
        /// <param name="canvasWidth">New width of the canvas</param>
        /// <param name="canvasHeight">New height of the canvas</param>
        public virtual void SetCanvasDimensions(int canvasWidth, int canvasHeight)
        {
            // Right now, it is unknown how many rectangles need to be placed.
            // So guess that a 1000 by 1000 capacity will be enough.
            const int initialCapacityX = 4096;
            const int initialCapacityY = 4096;

            // Initially, there is one free cell, which covers the entire canvas.
            _canvasCells.Initialize(initialCapacityX, initialCapacityY, canvasWidth, canvasHeight, new CanvasCell(false));

            _nbrCellsGenerated = 0;
            _nbrRectangleAddAttempts = 0;
            _lowestFreeHeightDeficitSinceLastRedim = Int32.MaxValue;

            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        /// <summary>
        /// Adds a rectangle
        /// </summary>
        /// <param name="rectangleWidth">Width of the rectangle</param>
        /// <param name="rectangleHeight">Height of the rectangle</param>
        /// <param name="rectangleXOffset">X position where rectangle has been placed</param>
        /// <param name="rectangleYOffset">Y position where rectangle has been placed</param>
        /// <param name="lowestFreeHeightDeficit">
        /// Lowest free height deficit for all the rectangles placed since the last call to SetCanvasDimensions.
        /// 
        /// This will be set to Int32.MaxValue if there was never any free height deficit.
        /// </param>
        /// <returns>
        /// true: rectangle placed
        /// false: rectangle not placed because there was no room
        /// </returns>
        public virtual bool AddRectangle(
            int rectangleWidth, int rectangleHeight, out int rectangleXOffset, out int rectangleYOffset,
            out int lowestFreeHeightDeficit)
        {
            rectangleXOffset = 0;
            rectangleYOffset = 0;
            lowestFreeHeightDeficit = Int32.MaxValue;

            int requiredWidth = rectangleWidth;
            int requiredHeight = rectangleHeight;

            _nbrRectangleAddAttempts++;

            int x = 0;
            int y = 0;
            int offsetX = 0;
            int offsetY = 0;
            bool rectangleWasPlaced = false;
            int nbrRows = _canvasCells.NbrRows;

            do
            {
                int nbrRequiredCellsHorizontally;
                int nbrRequiredCellsVertically;
                int leftOverWidth;
                int leftOverHeight;

                // First move upwards until we find an unoccupied cell. 
                // If we're already at an unoccupied cell, no need to do anything.
                // Important to clear all occupied cells to get 
                // the lowest free height deficit. This must be taken from the top of the highest 
                // occupied cell.

                while ((y < nbrRows) && (_canvasCells.Item(x, y).occupied))
                {
                    offsetY += _canvasCells.RowHeight(y);
                    y += 1;
                }

                // If we found an unoccupied cell, than see if we can place a rectangle there.
                // If not, than y popped out of the top of the canvas.

                if ((y < nbrRows) && (FreeHeightDeficit(_canvasHeight, offsetY, requiredHeight) <= 0))
                {
                    if (IsAvailable(
                        x, y, requiredWidth, requiredHeight,
                        out nbrRequiredCellsHorizontally, out nbrRequiredCellsVertically,
                        out leftOverWidth, out leftOverHeight))
                    {
                        PlaceRectangle(
                            x, y, requiredWidth, requiredHeight,
                            nbrRequiredCellsHorizontally, nbrRequiredCellsVertically,
                            leftOverWidth, leftOverHeight);

                        rectangleXOffset = offsetX;
                        rectangleYOffset = offsetY;

                        rectangleWasPlaced = true;
                        break;
                    }

                    // Go to the next cell
                    offsetY += _canvasCells.RowHeight(y);
                    y += 1;
                }

                // If we've come so close to the top of the canvas that there is no space for the
                // rectangle, go to the next column. This automatically also checks whether we've popped out of the top
                // of the canvas (in that case, _canvasHeight == offsetY).

                int freeHeightDeficit = FreeHeightDeficit(_canvasHeight, offsetY, requiredHeight);
                if (freeHeightDeficit > 0)
                {
                    offsetY = 0;
                    y = 0;

                    offsetX += _canvasCells.ColumnWidth(x);
                    x += 1;

                    // This update is far from perfect, because if the rectangle could not be placed at this column
                    // because of insufficient horizontal space, than this update should not be made (because it may lower
                    // _lowestFreeHeightDeficitSinceLastRedim while in raising the height of the canvas by this lowered amount
                    // may not result in the rectangle being placed here after all.
                    //
                    // However, checking for sufficient horizontal width takes a lot of CPU ticks. Tests have shown that this
                    // far outstrips the gains through having fewer failed sprite generations.
                    if (_lowestFreeHeightDeficitSinceLastRedim > freeHeightDeficit) { _lowestFreeHeightDeficitSinceLastRedim = freeHeightDeficit;  }
                }

                // If we've come so close to the right edge of the canvas that there is no space for
                // the rectangle, return false now.
                if ((_canvasWidth - offsetX) < requiredWidth)
                {
                    rectangleWasPlaced = false;
                    break;
                }
            } while (true);

            lowestFreeHeightDeficit = _lowestFreeHeightDeficitSinceLastRedim;
            return rectangleWasPlaced;
        }

        /// <summary>
        /// Works out the free height deficit when placing a rectangle with a required height at a given offset.
        /// 
        /// If the free height deficit is 0 or negative, there may be room to place the rectangle (still need to check for blocking
        /// occupied cells).
        /// 
        /// If the free height deficit is greater than 0, you're too close to the top edge of the canvas to place the rectangle.
        /// </summary>
        /// <param name="canvasHeight"></param>
        /// <param name="offsetY"></param>
        /// <param name="requiredHeight"></param>
        /// <returns></returns>
        private int FreeHeightDeficit(int canvasHeight, int offsetY, int requiredHeight)
        {
            int spaceLeftVertically = canvasHeight - offsetY;
            int freeHeightDeficit = requiredHeight - spaceLeftVertically;

            return freeHeightDeficit;
        }


        /// <summary>
        /// Sets the cell at x,y to occupied, and also its top and right neighbours, as needed
        /// to place a rectangle with the given width and height.
        /// 
        /// If the rectangle takes only part of a row or column, they are split.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="requiredWidth"></param>
        /// <param name="requiredHeight"></param>
        /// <param name="nbrRequiredCellsHorizontally">
        /// Number of cells that the rectangle requires horizontally
        /// </param>
        /// <param name="nbrRequiredCellsVertically">
        /// Number of cells that the rectangle requires vertically
        /// </param>
        /// <param name="leftOverWidth">
        /// The amount of horizontal space left in the right most cells that could be used for the rectangle
        /// </param>
        /// <param name="leftOverHeight">
        /// The amount of vertical space left in the bottom most cells that could be used for the rectangle
        /// </param>
        private void PlaceRectangle(
            int x, int y, 
            int requiredWidth, int requiredHeight,
            int nbrRequiredCellsHorizontally, int nbrRequiredCellsVertically,
            int leftOverWidth,
            int leftOverHeight)
        {
            // Split the far most row and column if needed.

            if (leftOverWidth > 0)
            {
                _nbrCellsGenerated += _canvasCells.NbrRows;

                int xFarRightColumn = x + nbrRequiredCellsHorizontally - 1;
                _canvasCells.InsertColumn(xFarRightColumn, leftOverWidth);
            }

            if (leftOverHeight > 0)
            {
                _nbrCellsGenerated += _canvasCells.NbrColumns;

                int yFarBottomColumn = y + nbrRequiredCellsVertically - 1;
                _canvasCells.InsertRow(yFarBottomColumn, leftOverHeight);
            }

            for (int i = x + nbrRequiredCellsHorizontally - 1; i >= x; i--)
            {
                for (int j = y + nbrRequiredCellsVertically - 1; j >= y; j--)
                {
                    _canvasCells.SetItem(i, j, new CanvasCell(true));
                }
            }
        }

        /// <summary>
        /// Returns true if a rectangle with the given width and height can be placed
        /// in the cell with the given x and y, and its right and top neighbours.
        /// 
        /// This method assumes that x,y is far away enough from the edges of the canvas
        /// that the rectangle could actually fit. So this method only looks at whether cells
        /// are occupied or not.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="requiredWidth"></param>
        /// <param name="requiredHeight"></param>
        /// <param name="nbrRequiredCellsHorizontally">
        /// Number of cells that the rectangle requires horizontally
        /// </param>
        /// <param name="nbrRequiredCellsVertically">
        /// Number of cells that the rectangle requires vertically
        /// </param>
        /// <param name="leftOverWidth">
        /// The amount of horizontal space left in the right most cells that could be used for the rectangle
        /// </param>
        /// <param name="leftOverHeight">
        /// The amount of vertical space left in the bottom most cells that could be used for the rectangle
        /// </param>
        /// <returns></returns>
        private bool IsAvailable(
            int x, int y, int requiredWidth, int requiredHeight, 
            out int nbrRequiredCellsHorizontally,
            out int nbrRequiredCellsVertically,
            out int leftOverWidth,
            out int leftOverHeight)
        {
            nbrRequiredCellsHorizontally = 0;
            nbrRequiredCellsVertically = 0;
            leftOverWidth = 0;
            leftOverHeight = 0;

            int foundWidth = 0;
            int foundHeight = 0;
            int trialX = x;
            int trialY = y;

            // Check all cells that need to be unoccupied for there to be room for the rectangle.

            while (foundHeight < requiredHeight)
            {
                trialX = x;
                foundWidth = 0;

                while (foundWidth < requiredWidth)
                {
                    if (_canvasCells.Item(trialX, trialY).occupied)
                    {
                        return false;
                    }

                    foundWidth += _canvasCells.ColumnWidth(trialX);
                    trialX++;
                }

                foundHeight += _canvasCells.RowHeight(trialY);
                trialY++;
            }

            // Visited all cells that we'll need to place the rectangle,
            // and none were occupied. So the space is available here.

            nbrRequiredCellsHorizontally = trialX - x;
            nbrRequiredCellsVertically = trialY - y;

            leftOverWidth = (foundWidth - requiredWidth);
            leftOverHeight = (foundHeight - requiredHeight);

            return true;
        }

        /// <summary>
        /// The canvas keeps statistics, on for example the number of times a FreeAreas is generated.
        /// Use this method to fill an object that implements ICanvasStats with these statistics.
        /// 
        /// Note that calling SetCanvasDimensions resets all counters.
        /// </summary>
        /// <param name="canvasStats">
        /// Reference to object to be filled.
        /// 
        /// If this is null, nothing happens (so there is no exception).
        /// </param>
        public void GetStatistics(CanvasStats canvasStats)
        {
            canvasStats.NbrCellsGenerated = _nbrCellsGenerated;
            canvasStats.RectangleAddAttempts = _nbrRectangleAddAttempts;
            canvasStats.LowestFreeHeightDeficit = _lowestFreeHeightDeficitSinceLastRedim;
        }
    }
}
