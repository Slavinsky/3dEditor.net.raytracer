﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics;
using System.Threading;
using System.Runtime.Serialization;

namespace RayTracerLib
{
    [DataContract]
    public class Cylinder : DefaultShape
    {
        /// <summary>
        /// Stred
        /// </summary>
        [DataMember]
        public Vektor Center { get; private set; }

        /// <summary>
        /// spodni stred valce
        /// </summary>
        private Vektor CenterLow { get; set; }

        /// <summary>
        /// smer osy valce - puvodni
        /// </summary>
        [DataMember]
        public Vektor Dir { get; private set; }

        /// <summary>
        /// smer osy - normalizovany
        /// </summary>
        private Vektor DirNorm { get; set; }


        /// <summary>
        /// polomer
        /// </summary>
        [DataMember]
        public double Rad { get; private set; }
        /// <summary>
        /// vyska
        /// </summary>
        [DataMember]
        public double Height { get; private set; }

        #region PrivateTempAtributes
        
        /// <summary>
        /// osova primka: L= C + D*h
        /// </summary>
        Vektor L;
        /// <summary>
        /// rovina prvni podstavy
        /// </summary>
        Plane _plane1;
        /// <summary>
        /// stred prvni podstavy
        /// </summary>
        Vektor _c1;
        /// <summary>
        /// rovina druhe podstavy
        /// </summary>
        Plane _plane2;
        /// <summary>
        /// stred druhe podstavy
        /// </summary>
        Vektor _c2;

        #endregion


        public Cylinder() : this(new Vektor(0, 0, 0), new Vektor(1, 0, 0), 1, 3) { }
        /// <summary>
        /// vytvori novy valec
        /// </summary>
        /// <param name="center">uplny stred valce</param>
        /// <param name="direction">smer osy valce</param>
        /// <param name="radius">polomer</param>
        /// <param name="heigh">vyska</param>
        public Cylinder(Vektor center, Vektor direction, double radius, double heigh)
        {
            SetLabelPrefix("cylind");
            IsActive = true;
            this.Material = new Material();
            SetValues(center, direction, radius, heigh);
        }

        public Cylinder(Vektor center, Vektor direction, double radius, double heigh, bool nolabel)
        {
            IsActive = true;
            this.Material = new Material();
            SetValues(center, direction, radius, heigh);
        }
        public Cylinder(Cylinder old)
            : base(old)
        {
            IsActive = old.IsActive;
            Center = new Vektor(old.Center);
            CenterLow = new Vektor(old.CenterLow);
            DirNorm = new Vektor(old.DirNorm);
            Dir = new Vektor(old.Dir);
            Rad = old.Rad;
            Height = old.Height;
            L = new Vektor(old.L);
            Material = new Material(old.Material);
            _plane1 = new Plane(old._plane1);
            _c1 = new Vektor(old._c1);
            _plane2 = new Plane(old._plane2);
            _c2 = new Vektor(old._c2);
        }


        /// <summary>
        /// hromadne prenastavi hodnoty
        /// </summary>
        /// <param name="center">uplny stred valce</param>
        /// <param name="direction">smer osy valce</param>
        /// <param name="radius">polomer</param>
        /// <param name="heigh">vyska</param>
        public void SetValues(Vektor center, Vektor direction, double radius, double heigh)
        {
            Center = new Vektor(center);
            Dir = new Vektor(direction);
            DirNorm = new Vektor(direction);
            DirNorm.Normalize();
            Rad = radius;
            Height = heigh;
            CenterLow = new Vektor(center - DirNorm * (Height / 2));
            //CenterLow = new Vektor(center);
            L = new Vektor(CenterLow);
            L = L + DirNorm * Height;

            Vektor C1 = this.Center + DirNorm * (Height / 2);
            Vektor Norm1 = new Vektor(DirNorm);
            double D1 = C1 * Norm1;
            _plane1 = new Plane(Norm1, -D1, this.Material);
            _c1 = C1;

            Vektor C2 = this.Center - DirNorm * (Height / 2);
            Vektor Norm2 = new Vektor(Vektor.ZeroVektor - DirNorm);
            double D2 = C2 * Norm2;
            _plane2 = new Plane(Norm2, -D2, this.Material);
            _c2 = C2;


            _ShiftMatrix = Matrix3D.PosunutiNewMatrix(center.X, center.Y, center.Z);

            Vektor yAxe = new Vektor(0, 1, 0);
            Quaternion q = new Quaternion(yAxe, new Vektor(direction));
            double[] degss = q.ToEulerDegs();

            // rotace z kvaternionu je opacne orientovana, proto minuska
            Matrix3D matCr = Matrix3D.NewRotateByDegrees(-degss[0], -degss[1], -degss[2]);
            _RotatMatrix = matCr;
            _localMatrix = _RotatMatrix * _ShiftMatrix;

        }

        public override bool Intersects(Vektor P0, Vektor Pd, ref List<SolidPoint> InterPoint, bool isForLight, double lightDist)
        {
            if (!IsActive)
                return false;

            if (isForLight && InterPoint.Count > 0)
            {
                foreach (SolidPoint solp in InterPoint)
                    if (lightDist > solp.T) return true;
            }
            Interlocked.Increment(ref DefaultShape.TotalTested);

            // 1) spocitani pruniku s podstavami
            // rovina jedne podstavy: C*Norm = D
            // musi pak platit: C * Norm + D = 0
            // vime: C .. stred roviny
            //       Norm .. normala roviny = Dir nebo -Dir
            // chceme: D
            bool toReturn = false;
            if (_plane1 == null)
            {
                _c1 = this.Center + DirNorm * (Height / 2);
                Vektor Norm1 = new Vektor(DirNorm);
                double D1 = _c1 * Norm1;

                _plane1 = new Plane(Norm1, -D1, this.Material);
            }
            List<SolidPoint> BasePoints = new List<SolidPoint>();
            _plane1.Intersects(P0, Pd, ref BasePoints, isForLight, lightDist);
            foreach (SolidPoint sps in BasePoints)
            {
                if (MyMath.Distance2Points(sps.Coord, _c1) <= this.Rad)
                {
                    sps.Shape = this;
                    sps.Material = this.Material;
                    InterPoint.Add(sps);
                    toReturn = true;
                }
            }

            if (_plane2 == null)
            {
                _c2 = this.Center - DirNorm * (Height / 2);
                Vektor Norm2 = new Vektor(Vektor.ZeroVektor - DirNorm);
                double D2 = _c2 * Norm2;

                _plane2 = new Plane(Norm2, -D2, this.Material);
            }
            BasePoints.Clear();
            _plane2.Intersects(P0, Pd, ref BasePoints, isForLight, lightDist);

            foreach (SolidPoint sps in BasePoints)
            {
                if (MyMath.Distance2Points(sps.Coord, _c2) <= this.Rad)
                {
                    sps.Shape = this;
                    sps.Material = this.Material;
                    InterPoint.Add(sps);
                    toReturn = true;
                }
            }


            ////////////////////////////////////////
            // 2) spocitani pruniku s plastem
            Vektor P1 = new Vektor(Pd);
            P1.Normalize();
            
            // pocatek noveho paprsku
            Vektor RayP0 = new Vektor(P0);
            double skalar = DirNorm * P0;
            RayP0 = P0 - DirNorm * skalar;

            // smer noveho paprsku
            Vektor RayP1 = new Vektor(DirNorm);
            skalar = DirNorm * P1;
            RayP1 = P1 - DirNorm * skalar;

            // stred nove kruznice
            Vektor Center2 = new Vektor(CenterLow);
            skalar = DirNorm * CenterLow;
            Center2 = Center2 - DirNorm * skalar;

            double A = RayP1 * RayP1;
            double B = 2 * (RayP0 * RayP1 - RayP1 * Center2);
            double C = (RayP0 - Center2) * (RayP0 - Center2) - Rad * Rad;

            double discr = B * B - 4 * A * C;
            if (discr < MyMath.EPSILON)
                return toReturn;

            discr = Math.Sqrt(discr);
            double jmenovatel = 2 * A;
            double t0 = (-B - discr) / jmenovatel;
            double t1 = (-B + discr) / jmenovatel;

            double tMin = Math.Min(t0, t1);
            double tMax = Math.Max(t0, t1);

            double t = tMin;
            if (tMin < MyMath.EPSILON)
                t = tMax;
            if (t < MyMath.EPSILON)
                return toReturn;

            SolidPoint sp = new SolidPoint();
            sp.T = t;
            sp.Coord = P0 + Pd * t;

            Vektor PC = sp.Coord - CenterLow;
            Vektor PC2 = Vektor.Projection(DirNorm, PC);

            if (PC2 * DirNorm < MyMath.EPSILON || PC2.Size() > Height)
                return toReturn;

            //sp.Shape = new Sphere(PC2, this.R);
            Vektor v1 = sp.Coord - CenterLow;
            Vektor v2 = DirNorm * (v1 * DirNorm);
            sp.Normal = v1 - v2;
            sp.Normal.Normalize();

            if (sp.Normal * P1 > 0)
                sp.Normal = Vektor.ZeroVektor - sp.Normal;

            sp.Color = new Colour(this.Material.Color);
            sp.Material = this.Material;
            sp.Shape = this;

            InterPoint.Add(sp);


            return true;
        }

        public override void Move(double dx, double dy, double dz)
        {
            throw new NotImplementedException();
        }

        public override void MoveToPoint(double dx, double dy, double dz)
        {
            //Vektor dVec = new Vektor(dx, dy, dz);
            //this.Origin += dVec;
            this.Center.X = dx;
            this.Center.Y = dy;
            this.Center.Z = dz;
            this.SetValues(Center, Dir, Rad, Height);
        }

        public override string ToString()
        {
            return Label + " {Center=" + Center + "; Dir=" + Dir + "}";
        }

        public override void Rotate(double degAroundX, double degAroundY, double degAroundZ)
        {
            Matrix3D newRot = Matrix3D.NewRotateByDegrees(degAroundX, degAroundY, degAroundZ);

            Vektor yAxe = new Vektor(0, 1, 0);
            newRot.TransformPoint(yAxe);

            // 1) pretransformovat vsechny vektory do puvodniho (zakladniho) tvaru
            // nejspis to neni potreba, jelikoz je vypocitavame cele znovu
            //this.Peak = transpLoc.Transform2NewPoint(this.Peak);
            //this.Center = transpLoc.Transform2NewPoint(this.Center);
            //this.Dir = transpLoc.Transform2NewPoint(this.Dir);

            // 2) nastavit novou matici
            this._RotatMatrix = newRot;
            _localMatrix = _RotatMatrix * _ShiftMatrix;

            // 3) prenastavit objekt podle nove matice
            this.SetValues(this.Center, yAxe, this.Rad, this.Height);
        }

        public override DefaultShape FromDeserial()
        {
            Cylinder cyl = new Cylinder(this.Center, this.Dir, this.Rad, this.Height, true);
            if (this.Label != null)
                cyl.Label = this.Label;
            cyl.IsActive = this.IsActive;
            cyl.Material = this.Material;
            return cyl;
        }
    }
}
