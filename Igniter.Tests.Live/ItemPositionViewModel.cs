using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Igniter.Tests.Live
{
    public class ItemPositionViewModel
    {
        public ItemPositionViewModel()
        {
            SimpleList = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => i.ToString()));

            GroupedList = new ListCollectionView(Enumerable.Range(0, 10).Select(i => new GroupableItem(i)).ToList());
            GroupedList.GroupDescriptions.Add(new PropertyGroupDescription("ModFive"));
        }

        public ObservableCollection<string> SimpleList { get; private set; }

        public ListCollectionView GroupedList { get; private set; }

        private struct GroupableItem
        {
            private readonly int _val;

            public GroupableItem(int val)
            {
                _val = val;
            }

            public int ModFive
            {
                get { return _val % 6; }
            }

            public override string ToString()
            {
                return _val.ToString();
            }
        }
    }
}