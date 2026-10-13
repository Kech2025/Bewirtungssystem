using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Bewirtung
{
    /// <summary>
    /// Hilfsfunktionen rund um das Sortieren von DataGridViews.
    /// </summary>
    public static class GridSortHelper
    {
        /// <summary>
        /// Aktiviert die Sortierung per Klick auf die Spaltenüberschrift für alle Spalten des Grids.
        /// </summary>
        public static void EnableHeaderSorting(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }
    }

    /// <summary>
    /// Eine <see cref="BindingList{T}"/>, die das Sortieren per Klick auf die
    /// Spaltenüberschrift in einer <see cref="System.Windows.Forms.DataGridView"/> unterstützt.
    /// </summary>
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        private PropertyDescriptor? _sortProperty;

        public SortableBindingList()
        {
        }

        public SortableBindingList(IEnumerable<T> enumeration)
            : base(enumeration.ToList())
        {
        }

        protected override bool SupportsSortingCore => true;

        protected override bool IsSortedCore => _isSorted;

        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override PropertyDescriptor? SortPropertyCore => _sortProperty;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            var items = (List<T>)Items;
            var sorted = direction == ListSortDirection.Ascending
                ? items.OrderBy(x => prop.GetValue(x)).ToList()
                : items.OrderByDescending(x => prop.GetValue(x)).ToList();

            items.Clear();
            items.AddRange(sorted);

            _isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
        }
    }
}
