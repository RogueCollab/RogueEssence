using System;
using System.Text;
using System.Diagnostics;

namespace RectPacker
{
    /// <summary>
    /// Implements a two dimensional dynamic array with elements of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicTwoDimensionalArray<T>
    {
        /// <summary>
        /// Describes a row or column
        /// </summary>
        private struct Dimension
        {
            public short _size;
            public short _index;

            // The width of a column or the height of a row
            public int Size
            {
                get { return (int)_size; }
                set { _size = (short)value; }
            }

            // When a row or column is split, the new row is created at the end of the physical array rather than in the middle.
            // That way, there is no need to copy lots of data. But it does mean you need indirection from the logical index
            // to the physical index.
            // This field provides the physical index.
            public int Index
            {
                get { return (int)_index; }
                set { _index = (short)value; }
            }
        }

        // Describe the rows and columns
        private Dimension[] _columns;
        private Dimension[] _rows;

        private T[,] _data;

        // Number of logical columns in the 2 dimensional array
        private int _nbrColumns = 0;

        // Number of logical rows in the 2 dimensional array
        private int _nbrRows = 0;

        /// <summary>
        /// Number of columns
        /// </summary>
        public int NbrColumns { get { return _nbrColumns; } }

        /// <summary>
        /// Number of rows
        /// </summary>
        public int NbrRows { get { return _nbrRows; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicTwoDimensionalArray()
        {
        }

        /// <summary>
        /// After you've constructed the array, you need to initialize it.
        /// 
        /// This removes any content and creates the first cell - so the array
        /// will have height is 1 and width is 1.
        /// </summary>
        /// <param name="capacityX">
        /// The array will initially have capacity for at least this many columns. 
        /// Must be greater than 0.
        /// Set to the expected maximum width of the array or greater.
        /// The array will resize itself if you make this too small, but resizing is expensive.
        /// </param>
        /// <param name="capacityY">
        /// The array will initially have capacity for at least this many rows.
        /// Must be greater than 0.
        /// Set to the expected maximum height of the array or greater.
        /// The array will resize itself if you make this too small, but resizing is expensive.
        /// </param>
        /// <param name="firstColumnWidth">
        /// Width of the first column.
        /// </param>
        /// <param name="firstRowHeight">
        /// Width of the first column.
        /// </param>
        /// <param name="firstCellValue">
        /// Width of the first column.
        /// </param>
        public void Initialize(int capacityX, int capacityY, int firstColumnWidth, int firstRowHeight, T firstCellValue)
        {
            if (capacityX <= 0) { throw new Exception("capacityX cannot be 0 or smaller"); }
            if (capacityY == 0) { throw new Exception("capacityY cannot be 0 or smaller"); }

            if ((_columns == null) || (_columns.GetLength(0) < capacityX))
            {
                _columns = new Dimension[capacityX];
            }

            if ((_rows == null) || (_rows.GetLength(0) < capacityY))
            {
                _rows = new Dimension[capacityY];
            }

            if ((_data == null) || (_data.GetLength(0) < capacityX) || (_data.GetLength(1) < capacityY))
            {
                _data = new T[capacityX, capacityY];
            }

            _nbrColumns = 1;
            _nbrRows = 1;

            _columns[0].Index = 0;
            _columns[0].Size = firstColumnWidth;

            _rows[0].Index = 0;
            _rows[0].Size = firstRowHeight;

            _data[0, 0] = firstCellValue;
        }

        /// <summary>
        /// Returns the item at the given location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T Item(int x, int y)
        {
            return _data[_columns[x].Index, _rows[y].Index];
        }

        /// <summary>
        /// Sets an item to the given value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void SetItem(int x, int y, T value)
        {
            _data[_columns[x].Index, _rows[y].Index] = value;
        }

        /// <summary>
        /// Inserts a row at location y.
        /// If y equals 2, than all rows at y=3 and higher will now have y=4 and higher.
        /// The new row will have y=3.
        /// The contents of the row at y=2 will be copied to the row at y=3.
        /// 
        /// If there is not enough capacity in the array for the additional row,
        /// than the internal data structure will be copied to a structure with twice the size
        /// (this copying is expensive).
        /// </summary>
        /// <param name="y">
        /// Identifies the row to be split.
        /// </param>
        /// <param name="heightNewRow">
        /// The height of the new row (the one at y=3 in the example).
        /// Must be smaller than the current height of the existing row.
        /// 
        /// The old row will have height = (old height of old row) - (height of new row). 
        /// </param>
        public void InsertRow(int y, int heightNewRow)
        {
            if (y >= _nbrRows) { throw new Exception(string.Format("y is {0} but height is only {1}", y, _nbrRows)); } 

            // If there are as many phyiscal rows as there are logical rows, we need to get more physical rows before the number
            // of logical rows can be increased.
            if (_data.GetLength(1) == _nbrRows) { IncreaseCapacity(); }

            // Copy the cells with the given y to a new row after the last used row. The y of the new row equals _nbrRows.
            int physicalY = _rows[y].Index;
            for (int x = 0; x < _nbrColumns; x++)
            {
                _data[x, _nbrRows] = _data[x, physicalY];
            }

            // Make room in the _rows array by shifting all items that come after the one indexed by y one position to the right.
            // If y is at the end of the array, there is no need to shift anything.
            if (y < (_nbrRows - 1)) { Array.Copy(_rows, y + 1, _rows, y + 2, (_nbrRows - y - 1)); }

            // Let the freed up element point at the newly copied row 
            _rows[y + 1].Index = _nbrRows;

            // Set the heights of the old and new rows.
            int oldHeight = _rows[y].Size;
            int newHeightExistingRow = oldHeight - heightNewRow;
            Debug.Assert((heightNewRow > 0) && (newHeightExistingRow > 0));
            _rows[y + 1].Size = heightNewRow;
            _rows[y].Size = newHeightExistingRow;

            // The logical height of the array has increased by 1.
            _nbrRows++;
        }

        /// <summary>
        /// Same as InsertRow, but than for columns.
        /// </summary>
        /// <param name="x"></param>
        public void InsertColumn(int x, int widthNewColumn)
        {
            if (x >= _nbrColumns) { throw new Exception(string.Format("x is {0} but width is only {1}", x, _nbrColumns)); } 

            // If there are as many phyiscal columns as there are logical columns, we need to get more physical columns before the number
            // of logical columns can be increased.
            if (_data.GetLength(0) == _nbrColumns) { IncreaseCapacity(); }

            // Copy the cells with the given x to a new column after the last used column. The x of the new column equals _nbrColumns.
            int nbrPhysicalRows = _data.GetLength(1);
            int physicalX = _columns[x].Index;
            Array.Copy(_data, physicalX * nbrPhysicalRows, _data, _nbrColumns * nbrPhysicalRows, _nbrRows);

            // Make room in the _columns array by shifting all items that come after the one indexed by x one position to the right.
            // If x is at the end of the array, there is no need to shift anything.
            if (x < (_nbrColumns - 1)) { Array.Copy(_columns, x + 1, _columns, x + 2, (_nbrColumns - x - 1)); }

            // Let the freed up element point at the newly copied column 
            _columns[x + 1].Index = _nbrColumns;

            // Set the widths of the old and new columns.
            int oldWidth = _columns[x].Size;
            int newWidthExistingColumn = oldWidth - widthNewColumn;
            Debug.Assert((widthNewColumn > 0) && (newWidthExistingColumn > 0));
            _columns[x + 1].Size = widthNewColumn;
            _columns[x].Size = newWidthExistingColumn;

            // The logical width of the array has increased by 1.
            _nbrColumns++;

        }

        /// <summary>
        /// Returns the width of the column at the given location
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int ColumnWidth(int x)
        {
            return _columns[x].Size;
        }

        /// <summary>
        /// Returns the height of the row at the given location
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int RowHeight(int y)
        {
            return _rows[y].Size;
        }

        /// <summary>
        /// Doubles the capacity of the internal data structures.
        /// 
        /// Creates a new array with double the width and height of the old array.
        /// Copies the element of the old array to the new array.
        /// Then replaces the old array with the new array.
        /// </summary>
        private void IncreaseCapacity()
        {
            int oldCapacityX = _data.GetLength(0);
            int oldCapacityY = _data.GetLength(1);

            int newCapacityX = oldCapacityX * 2;
            int newCapacityY = oldCapacityY * 2;
            int nbrItemsToCopy = oldCapacityX * oldCapacityY;

            T[,] newData = new T[newCapacityX, newCapacityY];
            Array.Copy(_data, newData, nbrItemsToCopy);

            _data = newData;
        }

        /// <summary>
        /// Represents the DynamicTowDimensionalArray as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();

            sb.Append(" X      ");
            for (int x = 0; x < _nbrColumns + 1; x++) { sb.AppendFormat("   {0,2:G} ", x); }
            sb.AppendLine();

            sb.Append("Y       ");
            for (int x = 0; x < _nbrColumns + 1; x++) { sb.AppendFormat(" ({0,3:G})", ColumnWidth(x)); }
            sb.AppendLine();

            for (int y = 0; y < _nbrRows + 1; y++)
            {
                sb.AppendFormat("{0,2:G} ({1,3:G}) ", y, RowHeight(y));

                for (int x = 0; x < _nbrColumns + 1; x++)
                {
                    sb.AppendFormat("   {0}  ", Item(x, y));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
