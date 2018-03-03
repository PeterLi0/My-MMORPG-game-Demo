
using System;
using System.Diagnostics;
using System.Globalization;

namespace Luna3D
{
    /// <summary>
    ///		Summary description for Quaternion.
    /// </summary>
    public struct Quaternion
    {


        private const float EPSILON = 1e-03f;

        public float w, x, y, z;

        private static readonly Quaternion identityQuat = new Quaternion(1.0f, 0.0f, 0.0f, 0.0f);
        private static readonly Quaternion zeroQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);

        private static readonly int[] next = new int[3]
                                             {
                                                 1, 2, 0
                                             };
        public static Quaternion identity
        {
            get
            {
                return new Quaternion(1f, 0f, 0f, 0f);
            }
        }




        //		public Quaternion()
        //		{
        //			this.w = 1.0f;
        //		}

        /// <summary>
        ///		Creates a new Quaternion.
        /// </summary>
        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }


        /// <summary>
        /// Used to multiply 2 Quaternions together.
        /// </summary>
        /// <remarks>
        ///		Quaternion multiplication is not communative in most cases.
        ///		i.e. p*q != q*p
        /// </remarks>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Multiply(Quaternion left, Quaternion right)
        {
            return left * right;
        }

        /// <summary>
        /// Used to multiply 2 Quaternions together.
        /// </summary>
        /// <remarks>
        ///		Quaternion multiplication is not communative in most cases.
        ///		i.e. p*q != q*p
        /// </remarks>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            var q = new Quaternion();

            q.w = left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z;
            q.x = left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y;
            q.y = left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z;
            q.z = left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x;

            /*
			return new Quaternion
				(
				left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z,
				left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y,
				left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z,
				left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x
				); */

            return q;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Multiply(Quaternion quat, Vector3 vector)
        {
            return quat * vector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 operator *(Quaternion quat, Vector3 vector)
        {
            // nVidia SDK implementation
            Vector3 uv, uuv;
            var qvec = new Vector3(quat.x, quat.y, quat.z);

            uv = Vector3.Cross(qvec, vector);
            uuv = Vector3.Cross(qvec, uv);
            uv *= (2.0f * quat.w);
            uuv *= 2.0f;

            return vector + uv + uuv;

            // get the rotation matrix of the Quaternion and multiply it times the vector
            //return quat.ToRotationMatrix() * vector;
        }

        /// <summary>
        /// Used when a float value is multiplied by a Quaternion.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Multiply(float scalar, Quaternion right)
        {
            return scalar * right;
        }

        /// <summary>
        /// Used when a float value is multiplied by a Quaternion.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator *(float scalar, Quaternion right)
        {
            return new Quaternion(scalar * right.w, scalar * right.x, scalar * right.y, scalar * right.z);
        }

        /// <summary>
        /// Used when a Quaternion is multiplied by a float value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Quaternion Multiply(Quaternion left, float scalar)
        {
            return left * scalar;
        }

        /// <summary>
        /// Used when a Quaternion is multiplied by a float value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Quaternion operator *(Quaternion left, float scalar)
        {
            return new Quaternion(scalar * left.w, scalar * left.x, scalar * left.y, scalar * left.z);
        }

        /// <summary>
        /// Used when a Quaternion is added to another Quaternion.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Add(Quaternion left, Quaternion right)
        {
            return left + right;
        }

        public static Quaternion Subtract(Quaternion left, Quaternion right)
        {
            return left - right;
        }

        /// <summary>
        /// Used when a Quaternion is added to another Quaternion.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            return new Quaternion(left.w + right.w, left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            return new Quaternion(left.w - right.w, left.x - right.x, left.y - right.y, left.z - right.z);
        }

        /// <summary>
        ///     Negates a Quaternion, which simply returns a new Quaternion
        ///     with all components negated.
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator -(Quaternion right)
        {
            return new Quaternion(-right.w, -right.x, -right.y, -right.z);
        }

        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return (left.w == right.w && left.x == right.x && left.y == right.y && left.z == right.z);
        }

        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !(left == right);
        }



        /// <summary>
        ///    An Identity Quaternion.
        /// </summary>
        public static Quaternion Identity
        {
            get
            {
                return identityQuat;
            }
        }

        /// <summary>
        ///    A Quaternion with all elements set to 0.0f;
        /// </summary>
        public static Quaternion Zero
        {
            get
            {
                return zeroQuat;
            }
        }

        /// <summary>
        ///		Squared 'length' of this quaternion.
        /// </summary>
        public float Norm
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w;
            }
        }

        /// <summary>
        ///    Local X-axis portion of this rotation.
        /// </summary>
        public Vector3 XAxis
        {
            get
            {
                var fTx = 2.0f * this.x;
                var fTy = 2.0f * this.y;
                var fTz = 2.0f * this.z;
                var fTwy = fTy * this.w;
                var fTwz = fTz * this.w;
                var fTxy = fTy * this.x;
                var fTxz = fTz * this.x;
                var fTyy = fTy * this.y;
                var fTzz = fTz * this.z;

                return new Vector3(1.0f - (fTyy + fTzz), fTxy + fTwz, fTxz - fTwy);
            }
        }

        /// <summary>
        ///    Local Y-axis portion of this rotation.
        /// </summary>
        public Vector3 YAxis
        {
            get
            {
                var fTx = 2.0f * this.x;
                var fTy = 2.0f * this.y;
                var fTz = 2.0f * this.z;
                var fTwx = fTx * this.w;
                var fTwz = fTz * this.w;
                var fTxx = fTx * this.x;
                var fTxy = fTy * this.x;
                var fTyz = fTz * this.y;
                var fTzz = fTz * this.z;

                return new Vector3(fTxy - fTwz, 1.0f - (fTxx + fTzz), fTyz + fTwx);
            }
        }

        /// <summary>
        ///    Local Z-axis portion of this rotation.
        /// </summary>
        public Vector3 ZAxis
        {
            get
            {
                var fTx = 2.0f * this.x;
                var fTy = 2.0f * this.y;
                var fTz = 2.0f * this.z;
                var fTwx = fTx * this.w;
                var fTwy = fTy * this.w;
                var fTxx = fTx * this.x;
                var fTxz = fTz * this.x;
                var fTyy = fTy * this.y;
                var fTyz = fTz * this.y;

                return new Vector3(fTxz + fTwy, fTyz - fTwx, 1.0f - (fTxx + fTyy));
            }
        }

        //public float PitchInDegrees
        //{
        //    get
        //    {
        //        return Mathf.RadiansToDegrees(Pitch);
        //    }
        //    set
        //    {
        //        Pitch = Mathf.DegreesToRadians(value);
        //    }
        //}

        //public float YawInDegrees
        //{
        //    get
        //    {
        //        return Mathf.RadiansToDegrees(Yaw);
        //    }
        //    set
        //    {
        //        Yaw = Mathf.DegreesToRadians(value);
        //    }
        //}

        //public float RollInDegrees
        //{
        //    get
        //    {
        //        return Mathf.RadiansToDegrees(Roll);
        //    }
        //    set
        //    {
        //        Roll = Mathf.DegreesToRadians(value);
        //    }
        //}

        public float Pitch
        {
            set
            {
                float pitch, yaw, roll;
                ToEulerAngles(out pitch, out yaw, out roll);
                this = FromEulerAngles(value, yaw, roll);
            }
            get
            {
                var test = this.x * this.y + this.z * this.w;
                if (Mathf.Abs(test) > 0.499f) // singularity at north and south pole
                {
                    return 0f;
                }
                return (float)Mathf.Atan2(2 * this.x * this.w - 2 * this.y * this.z, 1 - 2 * this.x * this.x - 2 * this.z * this.z);
            }
        }


        public float Yaw
        {
            set
            {
                float pitch, yaw, roll;
                ToEulerAngles(out pitch, out yaw, out roll);
                this = FromEulerAngles(pitch, value, roll);
            }
            get
            {
                var test = this.x * this.y + this.z * this.w;
                if (Mathf.Abs(test) > 0.499f) // singularity at north and south pole
                {
                    return Mathf.Sign(test) * 2 * Mathf.Atan2(this.x, this.w);
                }
                return Mathf.Atan2(2 * this.y * this.w - 2 * this.x * this.z, 1 - 2 * this.y * this.y - 2 * this.z * this.z);
            }
        }

        public float Roll
        {
            set
            {
                float pitch, yaw, roll;
                ToEulerAngles(out pitch, out yaw, out roll);
                this = FromEulerAngles(pitch, yaw, value);
            }
            get
            {
                var test = this.x * this.y + this.z * this.w;
                if (Mathf.Abs(test) > 0.499f) // singularity at north and south pole
                {
                    return Mathf.Sign(test) * Mathf.PI / 2;
                }
                return (float)Mathf.Asin(2 * test);
            }
        }


        public static Quaternion Slerp(float time, Quaternion quatA, Quaternion quatB)
        {
            //return Slerp(time, quatA, quatB, false);

            return Slerp(quatA, quatB, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="quatA"></param>
        /// <param name="quatB"></param>
        /// <param name="useShortestPath"></param>
        /// <returns></returns>
        public static Quaternion Slerp(float time, Quaternion quatA, Quaternion quatB, bool useShortestPath)
        {
            var cos = quatA.Dot(quatB);

            var angle = (float)Mathf.Acos(cos);

            if (Mathf.Abs(angle) < EPSILON)
            {
                return quatA;
            }

            var sin = Mathf.Sin(angle);
            if (sin == 0f)
                //return Quaternion.identity;
                sin = 0.0001f;

            var inverseSin = 1.0f / sin;
            var coeff0 = Mathf.Sin((1.0f - time) * angle) * inverseSin;
            var coeff1 = Mathf.Sin(time * angle) * inverseSin;

            Quaternion result;

            if (cos < 0.0f && useShortestPath)
            {
                coeff0 = -coeff0;
                // taking the complement requires renormalisation
                var t = coeff0 * quatA + coeff1 * quatB;
                t.Normalize();
                result = t;
            }
            else
            {
                result = (coeff0 * quatA + coeff1 * quatB);
            }

            return result;
        }

        public static Quaternion Slerp(Quaternion start, Quaternion end, float amount)
        {
            float opposite;
            float inverse;
            float dot = start.Dot(end);

            if (Math.Abs(dot) > 1.0f - 1e-6f)
            {
                inverse = 1.0f - amount;
                opposite = amount * Math.Sign(dot);
            }
            else
            {
                float acos = (float)Math.Acos(Math.Abs(dot));
                float invSin = (float)(1.0 / Math.Sin(acos));

                inverse = (float)Math.Sin((1.0f - amount) * acos) * invSin;
                opposite = (float)Math.Sin(amount * acos) * invSin * Math.Sign(dot);
            }

            Quaternion result = Quaternion.Identity;
            result.x = (inverse * start.x) + (opposite * end.x);
            result.y = (inverse * start.y) + (opposite * end.y);
            result.z = (inverse * start.z) + (opposite * end.z);
            result.w = (inverse * start.w) + (opposite * end.w);

            return result;
        }

        /// <overloads><summary>
        /// normalised linear interpolation - faster but less accurate (non-constant rotation velocity)
        /// </summary>
        /// <param name="fT"></param>
        /// <param name="rkP"></param>
        /// <param name="rkQ"></param>
        /// <returns></returns>
        /// </overloads>
        public static Quaternion Nlerp(float fT, Quaternion rkP, Quaternion rkQ)
        {
            return Nlerp(fT, rkP, rkQ, false);
        }


        public static Quaternion Nlerp(float fT, Quaternion rkP, Quaternion rkQ, bool shortestPath)
        {
            Quaternion result;
            var fCos = rkP.Dot(rkQ);
            if (fCos < 0.0f && shortestPath)
            {
                result = rkP + fT * ((-rkQ) - rkP);
            }
            else
            {
                result = rkP + fT * (rkQ - rkP);
            }
            result.Normalize();
            return result;
        }

        /// <summary>
        /// Creates a Quaternion from a supplied angle and axis.
        /// </summary>
        /// <param name="angle">Value of an angle in radians.</param>
        /// <param name="axis">Arbitrary axis vector.</param>
        /// <returns></returns>
        public static Quaternion FromAngleAxis(float angle, Vector3 axis)
        {
            var quat = new Quaternion();

            var halfAngle = 0.5f * angle;
            var sin = Mathf.Sin(halfAngle);

            quat.w = Mathf.Cos(halfAngle);
            quat.x = sin * axis.x;
            quat.y = sin * axis.y;
            quat.z = sin * axis.z;

            return quat;
        }

        public static Quaternion Squad(float t, Quaternion p, Quaternion a, Quaternion b, Quaternion q)
        {
            return Squad(t, p, a, b, q, false);
        }

        /// <summary>
        ///		Performs spherical quadratic interpolation.
        /// </summary>
        public static Quaternion Squad(float t, Quaternion p, Quaternion a, Quaternion b, Quaternion q, bool useShortestPath)
        {
            var slerpT = 2.0f * t * (1.0f - t);

            // use spherical linear interpolation
            var slerpP = Slerp(t, p, q, useShortestPath);
            var slerpQ = Slerp(t, a, b);

            // run another Slerp on the results of the first 2, and return the results
            return Slerp(slerpT, slerpP, slerpQ);
        }



        //public Vector3 ToEulerAnglesInDegrees()
        //{
        //    float pitch, yaw, roll;
        //    ToEulerAngles(out pitch, out yaw, out roll);
        //    return new Vector3(Mathf.RadiansToDegrees(pitch), Mathf.RadiansToDegrees(yaw),
        //                        Mathf.RadiansToDegrees(roll));
        //}

        public Vector3 ToEulerAngles()
        {
            float pitch, yaw, roll;
            ToEulerAngles(out pitch, out yaw, out roll);
            return new Vector3(pitch, yaw, roll);
        }

        //public void ToEulerAnglesInDegrees(out float pitch, out float yaw, out float roll)
        //{
        //    ToEulerAngles(out pitch, out yaw, out roll);
        //    pitch = Mathf.RadiansToDegrees(pitch);
        //    yaw = Mathf.RadiansToDegrees(yaw);
        //    roll = Mathf.RadiansToDegrees(roll);
        //}

        public void ToEulerAngles(out float pitch, out float yaw, out float roll)
        {
            var halfPi = Mathf.PI / 2;
            var test = this.x * this.y + this.z * this.w;
            if (test > 0.499f)
            {
                // singularity at north pole
                yaw = 2 * Mathf.Atan2(this.x, this.w);
                roll = halfPi;
                pitch = 0;
            }
            else if (test < -0.499f)
            {
                // singularity at south pole
                yaw = -2 * Mathf.Atan2(this.x, this.w);
                roll = -halfPi;
                pitch = 0;
            }
            else
            {
                var sqx = this.x * this.x;
                var sqy = this.y * this.y;
                var sqz = this.z * this.z;
                yaw = Mathf.Atan2(2 * this.y * this.w - 2 * this.x * this.z, 1 - 2 * sqy - 2 * sqz);
                roll = (float)Mathf.Asin(2 * test);
                pitch = Mathf.Atan2(2 * this.x * this.w - 2 * this.y * this.z, 1 - 2 * sqx - 2 * sqz);
            }

            if (pitch <= float.Epsilon)
            {
                pitch = 0f;
            }
            if (yaw <= float.Epsilon)
            {
                yaw = 0f;
            }
            if (roll <= float.Epsilon)
            {
                roll = 0f;
            }
        }

        //public static Quaternion FromEulerAnglesInDegrees(float pitch, float yaw, float roll)
        //{
        //    return FromEulerAngles(Mathf.DegreesToRadians(pitch), Mathf.DegreesToRadians(yaw),
        //                            Mathf.DegreesToRadians(roll));
        //}

        /// <summary>
        /// Combines the euler angles in the order yaw, pitch, roll to create a rotation quaternion
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        public static Quaternion FromEulerAngles(float pitch, float yaw, float roll)
        {
            return Quaternion.FromAngleAxis(yaw, Vector3.up) * Quaternion.FromAngleAxis(pitch, Vector3.right) *
                   Quaternion.FromAngleAxis(roll, Vector3.forward);

            /*TODO: Debug
			//Equation from http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
			//heading
			
			float c1 = (float)Math.Cos(yaw/2);
			float s1 = (float)Math.Sin(yaw/2);
			//attitude
			float c2 = (float)Math.Cos(roll/2);
			float s2 = (float)Math.Sin(roll/2);
			//bank
			float c3 = (float)Math.Cos(pitch/2);
			float s3 = (float)Math.Sin(pitch/2);
			float c1c2 = c1*c2;
			float s1s2 = s1*s2;

			float w =c1c2*c3 - s1s2*s3;
			float x =c1c2*s3 + s1s2*c3;
			float y =s1*c2*c3 + c1*s2*s3;
			float z =c1*s2*c3 - s1*c2*s3;
			return new Quaternion(w,x,y,z);*/
        }


        /// <summary>
        /// Performs a Dot Product operation on 2 Quaternions.
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public float Dot(Quaternion quat)
        {
            return this.w * quat.w + this.x * quat.x + this.y * quat.y + this.z * quat.z;
        }

        /// <summary>
        ///		Normalizes elements of this quaterion to the range [0,1].
        /// </summary>
        public void Normalize()
        {
            var factor = 1.0f / Mathf.Sqrt(Norm);

            this.w = this.w * factor;
            this.x = this.x * factor;
            this.y = this.y * factor;
            this.z = this.z * factor;
        }

        /// <summary>
        ///    
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public void ToAngleAxis(ref float angle, ref Vector3 axis)
        {
            // The quaternion representing the rotation is
            //   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)

            var sqrLength = this.x * this.x + this.y * this.y + this.z * this.z;

            if (sqrLength > 0.0f)
            {
                angle = 2.0f * (float)Mathf.Acos(this.w);
                var invLength = Mathf.InvSqrt(sqrLength);
                axis.x = this.x * invLength;
                axis.y = this.y * invLength;
                axis.z = this.z * invLength;
            }
            else
            {
                angle = 0.0f;
                axis.x = 1.0f;
                axis.y = 0.0f;
                axis.z = 0.0f;
            }
        }

        /// <summary>
        /// Gets a 3x3 rotation matrix from this Quaternion.
        /// </summary>
        /// <returns></returns>
        public Matrix3 ToRotationMatrix()
        {
            var rotation = new Matrix3();

            var tx = 2.0f * this.x;
            var ty = 2.0f * this.y;
            var tz = 2.0f * this.z;
            var twx = tx * this.w;
            var twy = ty * this.w;
            var twz = tz * this.w;
            var txx = tx * this.x;
            var txy = ty * this.x;
            var txz = tz * this.x;
            var tyy = ty * this.y;
            var tyz = tz * this.y;
            var tzz = tz * this.z;

            rotation.m00 = 1.0f - (tyy + tzz);
            rotation.m01 = txy - twz;
            rotation.m02 = txz + twy;
            rotation.m10 = txy + twz;
            rotation.m11 = 1.0f - (txx + tzz);
            rotation.m12 = tyz - twx;
            rotation.m20 = txz - twy;
            rotation.m21 = tyz + twx;
            rotation.m22 = 1.0f - (txx + tyy);

            return rotation;
        }

        /// <summary>
        /// Computes the inverse of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Inverse()
        {
            var norm = this.w * this.w + this.x * this.x + this.y * this.y + this.z * this.z;
            if (norm > 0.0f)
            {
                var inverseNorm = 1.0f / norm;
                return new Quaternion(this.w * inverseNorm, -this.x * inverseNorm, -this.y * inverseNorm, -this.z * inverseNorm);
            }
            else
            {
                // return an invalid result to flag the error
                return Quaternion.Zero;
            }
        }

        /// <summary>
        ///   Variant of Inverse() that is only valid for unit quaternions.
        /// </summary>
        /// <returns></returns>
        public Quaternion UnitInverse
        {
            get
            {
                return new Quaternion(this.w, -this.x, -this.y, -this.z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        public void ToAxes(out Vector3 xAxis, out Vector3 yAxis, out Vector3 zAxis)
        {
            xAxis = new Vector3();
            yAxis = new Vector3();
            zAxis = new Vector3();

            var rotation = ToRotationMatrix();

            xAxis.x = rotation.m00;
            xAxis.y = rotation.m10;
            xAxis.z = rotation.m20;

            yAxis.x = rotation.m01;
            yAxis.y = rotation.m11;
            yAxis.z = rotation.m21;

            zAxis.x = rotation.m02;
            zAxis.y = rotation.m12;
            zAxis.z = rotation.m22;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        public static Quaternion FromAxes(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis)
        {
            var rotation = new Matrix3();

            rotation.m00 = xAxis.x;
            rotation.m10 = xAxis.y;
            rotation.m20 = xAxis.z;

            rotation.m01 = yAxis.x;
            rotation.m11 = yAxis.y;
            rotation.m21 = yAxis.z;

            rotation.m02 = zAxis.x;
            rotation.m12 = zAxis.y;
            rotation.m22 = zAxis.z;

            // set this quaternions values from the rotation matrix built
            return FromRotationMatrix(rotation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        public static Quaternion FromRotationMatrix(Matrix3 matrix)
        {
            // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
            // article "Quaternion Calculus and Fast Animation".

            var result = Quaternion.Zero;

            var trace = matrix.m00 + matrix.m11 + matrix.m22;

            float root = 0.0f;

            if (trace > 0.0f)
            {
                // |this.w| > 1/2, may as well choose this.w > 1/2
                root = Mathf.Sqrt(trace + 1.0f); // 2w
                result.w = 0.5f * root;

                root = 0.5f / root; // 1/(4w)

                result.x = (matrix.m21 - matrix.m12) * root;
                result.y = (matrix.m02 - matrix.m20) * root;
                result.z = (matrix.m10 - matrix.m01) * root;
            }
            else
            {
                // |result.w| <= 1/2

                var i = 0;
                if (matrix.m11 > matrix.m00)
                {
                    i = 1;
                }
                if (matrix.m22 > matrix[i, i])
                {
                    i = 2;
                }

                var j = next[i];
                var k = next[j];

                root = Mathf.Sqrt(matrix[i, i] - matrix[j, j] - matrix[k, k] + 1.0f);

#if AXIOM_SAFE_ONLY
                var pi = 0.5f * root;
                root = 0.5f / root;                                                                                                       
                var pw = (matrix[k, j] - matrix[j, k]) * root;
                var pj = (matrix[j, i] + matrix[i, j]) * root;
                var pk = (matrix[k, i] + matrix[i, k]) * root;
			    result = i == 0
			                 ? new Quaternion(pw, pi, pj, pk)
			                 : i == 1
			                       ? new Quaternion(pw, pk, pi, pj)
			                       : new Quaternion(pw, pj, pk, pi);
#else
                unsafe
                {
                    var apkQuat = &result.x;

                    apkQuat[i] = 0.5f * root;
                    root = 0.5f / root;

                    result.w = (matrix[k, j] - matrix[j, k]) * root;

                    apkQuat[j] = (matrix[j, i] + matrix[i, j]) * root;
                    apkQuat[k] = (matrix[k, i] + matrix[i, k]) * root;
                }
#endif
            }

            return result;
        }

        /// <summary>
        ///		Calculates the logarithm of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Log()
        {
            // BLACKBOX: Learn this
            // If q = cos(A)+sin(A)*(x*i+y*j+z*k) where (x,y,z) is unit length, then
            // log(q) = A*(x*i+y*j+z*k).  If sin(A) is near zero, use log(q) =
            // sin(A)*(x*i+y*j+z*k) since sin(A)/A has limit 1.

            // start off with a zero quat
            var result = Quaternion.Zero;

            if (Mathf.Abs(this.w) < 1.0f)
            {
                var angle = (float)Mathf.Acos(this.w);
                var sin = Mathf.Sin(angle);

                if (Mathf.Abs(sin) >= EPSILON)
                {
                    var coeff = angle / sin;
                    result.x = coeff * this.x;
                    result.y = coeff * this.y;
                    result.z = coeff * this.z;
                }
                else
                {
                    result.x = this.x;
                    result.y = this.y;
                    result.z = this.z;
                }
            }

            return result;
        }

        /// <summary>
        ///		Calculates the Exponent of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Exp()
        {
            // If q = A*(x*i+y*j+z*k) where (x,y,z) is unit length, then
            // exp(q) = cos(A)+sin(A)*(x*i+y*j+z*k).  If sin(A) is near zero,
            // use exp(q) = cos(A)+A*(x*i+y*j+z*k) since A/sin(A) has limit 1.

            var angle = Mathf.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            var sin = Mathf.Sin(angle);

            // start off with a zero quat
            var result = Quaternion.Zero;

            result.w = Mathf.Cos(angle);

            if (Mathf.Abs(sin) >= EPSILON)
            {
                var coeff = sin / angle;

                result.x = coeff * this.x;
                result.y = coeff * this.y;
                result.z = coeff * this.z;
            }
            else
            {
                result.x = this.x;
                result.y = this.y;
                result.z = this.z;
            }

            return result;
        }


        /// <summary>
        ///		Overrides the Object.ToString() method to provide a text representation of 
        ///		a Quaternion.
        /// </summary>
        /// <returns>A string representation of a Quaternion.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Quaternion({0}, {1}, {2}, {3})", this.w, this.x, this.y, this.z);
        }

        public override int GetHashCode()
        {
            return (int)this.x ^ (int)this.y ^ (int)this.z ^ (int)this.w;
        }

        public override bool Equals(object obj)
        {
            var quat = (Quaternion)obj;

            return quat == this;
        }

        public bool Equals(Quaternion rhs, float tolerance)
        {
            var fCos = Dot(rhs);
            var angle = (float)Mathf.Acos(fCos);

            return Mathf.Abs(angle) <= tolerance;
        }

        public static Quaternion Parse(string quat)
        {
            // the format is "Quaternion(w, x, y, z)"
            if (!quat.StartsWith("Quaternion("))
            {
                throw new FormatException();
            }

            var values = quat.Substring(11).TrimEnd(')').Split(',');

            return new Quaternion(float.Parse(values[0], CultureInfo.InvariantCulture),
                                   float.Parse(values[1], CultureInfo.InvariantCulture),
                                   float.Parse(values[2], CultureInfo.InvariantCulture),
                                   float.Parse(values[3], CultureInfo.InvariantCulture));
        }

    }
}