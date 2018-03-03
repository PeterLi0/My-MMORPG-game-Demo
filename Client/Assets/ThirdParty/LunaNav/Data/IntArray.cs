using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class IntArray
	{
        public int[] Data { get; set; }
        public int Size { get; private set; }
        public int Capacity { get; set; }

        public IntArray()
        {
            Data = null;
            Size = 0;
            Capacity = 0;
        }

        public IntArray(int n) : base()
        {
            Resize(n);
        }

	    public void Resize(int n)
	    {
	        if (n > Capacity)
	        {
	            if(Capacity == 0) Capacity = 10;
	            while (Capacity < n) Capacity *= 2;
                int[] newData = new int[Capacity];
                if(Size > 0) Array.Copy(Data, 0, newData, 0, Size);
	            Data = newData;
	        }
	        Size = n;
	    }

        public void Push(int item)
        {
            Resize(Size + 1);
            Data[Size-1] = item;
        }

        public int Pop()
        {
            if (Size > 0)
                Size--;
            return Data[Size];
        }

        public int this[int i]
	    {
	        get { return Data[i]; }
            set { Data[i] = value; }
	    }

        public int[] ToArray()
        {
            return Data;
        }
    }
}
