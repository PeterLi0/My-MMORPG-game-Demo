using System;


namespace LunaNav
{
    public class ObstacleAvoidanceDebugData
    {
        private int _nsamples;
        private int _maxSamples;
        private float[] _vel;
        private float[] _ssize;
        private float[] _pen;
        private float[] _vpen;
        private float[] _vcpen;
        private float[] _spen;
        private float[] _tpen;

        public ObstacleAvoidanceDebugData()
        {
            _nsamples = 0;
            _maxSamples = 0;
            _vel = null;
            _ssize = null;
            _pen = null;
            _vpen = null;
            _vcpen = null;
            _spen = null;
            _tpen = null;
        }

        public bool Init(int maxSamples)
        {
            if(maxSamples <= 0)
                throw new ArgumentException("Max Samples must be larger than 0");
            _maxSamples = maxSamples;

            _vel = new float[maxSamples*3];
            _pen = new float[maxSamples];
            _ssize = new float[maxSamples];
            _vpen = new float[maxSamples];
            _vcpen = new float[maxSamples];
            _spen = new float[maxSamples];
            _tpen = new float[maxSamples];

            return true;
        }

        public void Reset()
        {
            _nsamples = 0;
        }

        public void AddSample(float[] vel, float ssize, float pen, float vpen, float vcpen, float spen, float tpen)
        {
            if (_nsamples >= _maxSamples)
                return;

            Array.Copy(_vel, _nsamples*3, vel, 0, 3);
            _ssize[_nsamples] = ssize;
            _pen[_nsamples] = pen;
            _vpen[_nsamples] = vpen;
            _vcpen[_nsamples] = vcpen;
            _spen[_nsamples] = spen;
            _tpen[_nsamples] = tpen;
            _nsamples++;
        }

        public void NormalizeSamples()
        {
            NormalizeArray(ref _pen, _nsamples);
            NormalizeArray(ref _vpen, _nsamples);
            NormalizeArray(ref _vcpen, _nsamples);
            NormalizeArray(ref _spen, _nsamples);
            NormalizeArray(ref _tpen, _nsamples);
        }

        private void NormalizeArray(ref float[] arr, int n)
        {
            float minPen = float.MaxValue;
            float maxPen = -float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                minPen = Math.Min(minPen, arr[i]);
                maxPen = Math.Max(maxPen, arr[i]);
            }

            float penRange = maxPen - minPen;
            float s = penRange > 0.001f ? (1.0f/penRange) : 1;
            for (int i = 0; i < n; i++)
            {
                arr[i] = Math.Max(0.0f, Math.Min(1.0f, (arr[i]-minPen)*s));
            }
        }

        public int SampleCount
        {
            get { return _nsamples; }
        }

        public float[] SampleVelocity(int i)
        {
            float[] ret = new float[3];
            Array.Copy(_vel, i*3, ret, 0, 3);
            return ret;
        }

        public float SampleSize(int i)
        {
            return _ssize[i];
        }

        public float SamplePenalty(int i)
        {
            return _pen[i];
        }

        public float SampleDesiredVelocityPenalty(int i)
        {
            return _vpen[i];
        }

        public float SampleCurrentVelocityPenalty(int i)
        {
            return _vcpen[i];
        }

        public float SamplePreferredSidePenalty(int i)
        {
            return _spen[i];
        }

        public float SampleCollisionTimePenalty(int i)
        {
            return _tpen[i];
        }
    }
}