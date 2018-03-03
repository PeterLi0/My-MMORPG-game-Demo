using System;

using System.Linq;

namespace LunaNav
{
    public class ProximityGrid
    {
        private int _maxItems;
        private float _cellSize;
        private float _invCellSize;

        class Item
        {
            public int id;
            public short x;
            public short y;
            public int next;
        }

        private Item[] _pool;
        private int _poolHead;
        private int _poolSize;

        private int[] _buckets;
        private int _bucketSize;

        private int[] _bounds = new int[4];

        public ProximityGrid()
        {
            _maxItems = 0;
            _cellSize = 0;
            _pool = null;
            _poolHead = 0;
            _poolSize = 0;
            _buckets = null;
            _bucketSize = 0;
        }

        public bool Init(int poolSize, float cellSize)
        {
            if(poolSize <= 0)
                throw new ArgumentException("Pool Size must be greater than 0");
            if(cellSize <= 0)
                throw new ArgumentException("Cell Size must be greater than 0");

            _cellSize = cellSize;
            _invCellSize = 1.0f/_cellSize;
            _bucketSize = (int)Helper.NextPow2(poolSize);
            _buckets = new int[_bucketSize];

            _poolSize = poolSize;
            _poolHead = 0;
            _pool = new Item[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                _pool[i] = new Item();
            }
            Clear();

            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _bucketSize; i++)
            {
                _buckets[i] = 0xffff;
            }
            _poolHead = 0;
            _bounds[0] = 0xffff;
            _bounds[1] = 0xffff;
            _bounds[2] = -0xffff;
            _bounds[3] = -0xffff;
        }

        public void AddItem(int id, float minx, float miny, float maxx, float maxy)
        {
            int iminx = (int)Math.Floor(minx * _invCellSize);
            int iminy = (int)Math.Floor(miny * _invCellSize);
            int imaxx = (int)Math.Floor(maxx * _invCellSize);
            int imaxy = (int)Math.Floor(maxy * _invCellSize);

            _bounds[0] = Math.Min(_bounds[0], iminx);
            _bounds[1] = Math.Min(_bounds[1], iminy);
            _bounds[2] = Math.Min(_bounds[2], imaxx);
            _bounds[3] = Math.Min(_bounds[3], imaxy);

            for (int y = iminy; y <= imaxy; y++)
            {
                for (int x = iminx; x <= imaxx; x++)
                {
                    if (_poolHead < _poolSize)
                    {
                        int h = HashPos2(x, y, _bucketSize);
                        int idx = _poolHead;
                        _poolHead++;
                        Item item = _pool[idx];
                        item.x = (short)x;
                        item.y = (short)y;
                        item.id = id;
                        item.next = _buckets[h];
                        _buckets[h] = idx;
                    }
                }
            }
        }

        public int QueryItems(float minx, float miny, float maxx, float maxy, ref int[] ids, int maxIds)
        {
            int iminx = (int)Math.Floor(minx * _invCellSize);
            int iminy = (int)Math.Floor(miny * _invCellSize);
            int imaxx = (int)Math.Floor(maxx * _invCellSize);
            int imaxy = (int)Math.Floor(maxy * _invCellSize);

            int n = 0;

            for (int y = iminy; y <= imaxy; y++)
            {
                for (int x = iminx; x <= imaxx; x++)
                {
                    int h = HashPos2(x, y, _bucketSize);
                    int idx = _buckets[h];
                    while (idx != 0xffff)
                    {
                        Item item = _pool[idx];
                        if (item.x == x && item.y == y)
                        {
                            if (!ids.Contains(item.id))
                            {
                                if (n >= maxIds)
                                    return n;
                                ids[n++] = item.id;
                            }
                        }
                        idx = item.next;
                    }
                }
            }
            return n;
        }

        public int GetItemCountAt(int x, int y)
        {
            int n = 0;
            int h = HashPos2(x, y, _bucketSize);
            int idx = _buckets[h];
            while (idx != 0xffff)
            {
                Item item = _pool[idx];
                if (item.x == x && item.y == y)
                    n++;
                idx = item.next;
            }

            return n;
        }

        public int[] Bounds
        {
            get { return _bounds; }
        }

        public float CellSize
        {
            get { return _cellSize; }
        }

        public int HashPos2(int x, int y, int n)
        {
            return ((x*73856093) ^ (y*19349663)) & (n - 1);
        }
    }
}