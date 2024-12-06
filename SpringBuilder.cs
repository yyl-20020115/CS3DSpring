using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CS3DSpring;

public static class SpringBuilder
{
    public const double _2PI = 2.0 * Math.PI;
    public static readonly Point3D origin = new(0, 0, 0);
    public static readonly Vector3D x_axis = new(1, 0, 0);
    public static readonly Vector3D y_axis = new(0, 1, 0);
    public static readonly Vector3D z_axis = new(0, 0, 1);
    public static readonly Vector3D[] axes = [x_axis, y_axis, z_axis];

    public struct PositionalVector
    {
        public Point3D Position;
        public Vector3D Normal;
    }

    public static double ToDegree(this double radian)
        => radian / Math.PI * 180.0;
    public static double GetIncludedAngle(Vector3D to, Vector3D from)
    {
        if (from == to) return _2PI;
        var cross = Vector3D.CrossProduct(from, to);
        cross.Normalize();
        var dot = Vector3D.DotProduct(from, to);
        var angle = Math.Atan2(cross.Length, dot);
        return cross.Z < 0 ? _2PI - angle : angle;
    }

    public static ModelVisual3D Render(this MeshGeometry3D geometry, Point3D? position = null, Vector3D? direction = null, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                },
                new SpotLight(light ?? Colors.White, position??new Point3D(),direction??new Vector3D(),10,5)
            };
        var material = new DiffuseMaterial(brush);
        collection.Add(new GeometryModel3D(geometry, material));
        group!.Children = collection;
        return model;

    }

    public static MeshGeometry3D BuildDonutMeshGeometry3D(double R = 50, int Segments = 360, double r = 10)
    {
        var Geometry = new MeshGeometry3D();

        double delta_angle = _2PI / Segments;
        double theta = 0.0;
        for (int i = 0; i < Segments; i++)
        {
            var phi = 0.0;
            for (int j = 0; j < Segments; j++)
            {
                Geometry.Positions.Add(new
                    ((R + r * Math.Cos(phi)) * Math.Cos(theta),
                     (R + r * Math.Cos(phi)) * Math.Sin(theta),
                     r * Math.Sin(phi)
                    ));
                phi += delta_angle;
            }
            theta += delta_angle;
        }

        BuildRingIndices(Geometry.TriangleIndices, Segments, Segments, true);

        return Geometry;
    }

    public static void BuildRingIndices(Int32Collection indices, int rings, int splits, bool close, bool flip = false)
    {
        for (int current_ring = 0; current_ring < (close ? rings : rings - 1); current_ring++)
        {
            int next_ring = (current_ring + 1) % rings;

            for (int current_point = 0; current_point < splits; current_point++)
            {
                int next_point = (current_point + 1) % splits;
                int p0 = current_ring * splits + current_point;
                int p1 = current_ring * splits + next_point;
                int p2 = next_ring * splits + current_point;
                int p3 = next_ring * splits + next_point;

                indices.Add(p1);
                if (flip)
                {
                    indices.Add(p2);
                    indices.Add(p0);
                }
                else
                {
                    indices.Add(p0);
                    indices.Add(p2);
                }
                indices.Add(p2);
                if (flip)
                {
                    indices.Add(p1);
                    indices.Add(p3);
                }
                else
                {
                    indices.Add(p3);
                    indices.Add(p1);
                }
            }
        }
    }

    public static MeshGeometry3D BuildSpringGeometry3D(int Coils = 4, double R = 50, int Segments = 360, double r = 10)
    {
        var Geometry = new MeshGeometry3D();

        double delta_angle = _2PI / Segments;
        double theta = 0.0;
        double delta_z = r * Coils / (Segments * Segments);
        double z = 0.0;

        for (int c = 0; c < Coils; c++)
        {
            for (int i = 0; i < Segments; i++)
            {
                var phi = 0.0;
                for (int j = 0; j < Segments; j++)
                {
                    Geometry.Positions.Add(new
                        ((R + r * Math.Cos(phi)) * Math.Cos(theta),
                         (R + r * Math.Cos(phi)) * Math.Sin(theta),
                         z + r * Math.Sin(phi)
                        ));
                    z += delta_z;
                    phi += delta_angle;
                }
                theta += delta_angle;
            }
        }


        BuildRingIndices(Geometry.TriangleIndices, Segments * Coils, Segments, false);

        Geometry.Positions.Add(new Point3D(R, 0, 0));
        Geometry.Positions.Add(new Point3D(R, 0, z));
        int first_center = Geometry.Positions.Count - 2;
        int last_center = Geometry.Positions.Count - 1;
        for (int current_point = 0; current_point < Segments - 1; current_point++)
        {
            int next_point = (current_point + 1) % Segments;

            Geometry.TriangleIndices.Add(current_point);
            Geometry.TriangleIndices.Add(next_point);
            Geometry.TriangleIndices.Add(first_center);
        }
        for (int current_point = Geometry.Positions.Count - 4; current_point >= Geometry.Positions.Count - 3 - Segments; current_point--)
        {
            int next_point = current_point + 1;

            Geometry.TriangleIndices.Add(next_point);
            Geometry.TriangleIndices.Add(current_point);
            Geometry.TriangleIndices.Add(last_center);
        }
        return Geometry;
    }

    public static MeshGeometry3D BuildCylinderGeometry3D(Point3D from, Point3D to, double radius, int coils, int rings)
    {
        var Geometry = new MeshGeometry3D();

        BuildCylinder(Geometry.Positions, from, to, radius, coils, rings);
        //make double sides
        BuildRingIndices(Geometry.TriangleIndices, coils, rings, false, true);
        BuildRingIndices(Geometry.TriangleIndices, coils, rings, false, false);
        return Geometry;
    }

    public static void BuildCylinder(Point3DCollection positions, Point3D from, Point3D to, double radius, int coils, int rings)
    {
        var axis = to - from;

        var main_height = (to - from).Length;
        var local_angle_round_delta = _2PI / rings;
        var delta_z = main_height / coils;


        var transform_group = new Transform3DGroup();
        var rotation = new AxisAngleRotation3D(axis, 0);

        var rotation_transform = new RotateTransform3D(rotation, from);
        var translate_transform = new TranslateTransform3D((Vector3D)from);

        transform_group.Children.Add(rotation_transform);
        transform_group.Children.Add(translate_transform);

        var z = 0.0;
        for (var j = 0; j < coils; j++)
        {
            var angle_round = 0.0;
            for (var i = 0; i < rings; i++)
            {
                var x = radius * Math.Cos(angle_round);
                var y = radius * Math.Sin(angle_round);
                angle_round += local_angle_round_delta;
                var p = new Point3D(x, y, z);
                positions.Add(transform_group.Transform(p));
            }
            z += delta_z;
        }
    }
    //获取和A，B构成平面垂直的向量
    public static Vector3D GetPerpendicularDirection(Vector3D vectorA, Vector3D vectorB)
    {
        var crossProduct = Vector3D.CrossProduct(vectorA, vectorB);

        // 归一化结果向量
        crossProduct.Normalize();

        return crossProduct;
    }
    /// <summary>
    /// 建立点环
    /// </summary>
    /// <param name="positions">结果点集</param>
    /// <param name="from">点本身</param>
    /// <param name="pointing">环的方向</param>
    /// <param name="radius">环的半径</param>
    /// <param name="splits">构成环的点数</param>
    public static void BuildPointRingHelper(Point3DCollection positions, Point3D from, Vector3D pointing, double radius, int splits)
    {
        var axis = GetPerpendicularDirection(pointing, z_axis);
        var angle = GetIncludedAngle(z_axis, pointing).ToDegree();

        var transform_group = new Transform3DGroup();
        //计算所知方向和z轴的角度
        var rotation = new AxisAngleRotation3D(axis, angle);
        var rotation_transform = new RotateTransform3D(rotation, origin);
        var translate_transform = new TranslateTransform3D(from.X, from.Y, from.Z);
        //定位点环位置本身的操作是先转动后移动
        transform_group.Children.Add(rotation_transform);
        transform_group.Children.Add(translate_transform);

        var angle_round = 0.0;
        var angle_delta = _2PI / splits;

        for (var i = 0; i < splits; i++)
        {
            var x = radius * Math.Cos(angle_round);
            var y = radius * Math.Sin(angle_round);
            var p = new Point3D(x, y, 0);
            positions.Add(transform_group.Transform(p));
            angle_round += angle_delta;
        }
    }
    public static void BuildCubeHelper(Point3DCollection positions, Int32Collection indices, Point3D from, Vector3D pointing, double radius = 1, double ratio = 8)
    {
        var axis = GetPerpendicularDirection(pointing, z_axis);
        var angle = GetIncludedAngle(z_axis, pointing).ToDegree();

        var transform_group = new Transform3DGroup();
        //计算所知方向和z轴的角度
        var rotation = new AxisAngleRotation3D(axis, angle);
        var rotation_transform = new RotateTransform3D(rotation, origin);
        var translate_transform = new TranslateTransform3D(from.X, from.Y, from.Z);
        //定位点环位置本身的操作是先转动后移动
        transform_group.Children.Add(rotation_transform);
        transform_group.Children.Add(translate_transform);

        ratio /= 2.0;
        //  3   2
        //  0   1
        //
        //  7   6 
        //  5   4 
        Point3D[] points = [
            new (-radius,-radius,-ratio*radius),
            new (+radius,-radius,-ratio*radius),
            new (+radius,+radius,-ratio*radius),
            new (-radius,+radius,-ratio*radius),

            new (-radius,-radius,+ratio*radius),
            new (+radius,-radius,+ratio*radius),
            new (+radius,+radius,+ratio*radius),
            new (-radius,+radius,+ratio*radius),
            ];

        foreach (var point in points)
        {
            positions.Add(transform_group.Transform(point));
        }
        Int32Collection _indices = [
            5,6,7, //front
            7,4,5, //
            3,2,1, //back
            1,0,3,
            2,3,7, //top
            7,6,2,
            0,1,5, //bottom
            5,4,0,
            2,6,5, //right
            5,1,2,
            3,0,7, //left
            7,0,4
            ];
        var _base = positions.Count;

        foreach (var index in _indices)
        {
            indices.Add(index + _base);
        }
    }

    public static MeshGeometry3D BuildSpringDonutWithCubesGeometry3D(double SR = 500, int SR_rings = 24, double R = 200, int R_rings = 30, double r = 2, double ratio = 16)
    {
        var Geometry = new MeshGeometry3D();

        var R_angle_step = 360.0 / R_rings;
        var SR_R_step_angle = 360.0 / (R_rings * SR_rings);

        var transform_group = new Transform3DGroup();

        var R_rotation = new AxisAngleRotation3D(y_axis, 0);
        var R_rotation_transform = new RotateTransform3D(R_rotation, origin);
        var R_translate_transform = new TranslateTransform3D(R, 0, 0);

        var SR_rotation = new AxisAngleRotation3D(z_axis, 0);
        var SR_rotation_transform = new RotateTransform3D(SR_rotation, origin);
        var SR_translate_transform = new TranslateTransform3D(SR, 0, 0);

        //定位点的操作是先局部后整体先移动后转动
        transform_group.Children.Add(R_translate_transform);
        transform_group.Children.Add(R_rotation_transform);
        transform_group.Children.Add(SR_translate_transform);
        transform_group.Children.Add(SR_rotation_transform);

        for (var j = 0; j < SR_rings; j++)
        {
            for (var i = 0; i < R_rings; i++)
            {
                var start = transform_group.Transform(origin);

                R_rotation.Angle += R_angle_step;
                SR_rotation.Angle += SR_R_step_angle;

                var end = transform_group.Transform(origin);

                BuildCubeHelper(Geometry.Positions, Geometry.TriangleIndices, start, end - start, r, ratio);
            }
        }

        return Geometry;
    }


    public static MeshGeometry3D BuildSpringDonutGeometry3D(double SR = 500, int SR_rings = 48, double R = 100, int R_rings = 120, double r = 10, int splits = 30)
    {
        var Geometry = new MeshGeometry3D();

        var R_angle_step = 360.0 / R_rings;
        var SR_R_step_angle = 360.0 / (R_rings * SR_rings);

        var transform_group = new Transform3DGroup();

        var R_rotation = new AxisAngleRotation3D(y_axis, 0);
        var R_rotation_transform = new RotateTransform3D(R_rotation, origin);
        var R_translate_transform = new TranslateTransform3D(R, 0, 0);

        var SR_rotation = new AxisAngleRotation3D(z_axis, 0);
        var SR_rotation_transform = new RotateTransform3D(SR_rotation, origin);
        var SR_translate_transform = new TranslateTransform3D(SR, 0, 0);

        //定位点的操作是先局部后整体先移动后转动
        transform_group.Children.Add(R_translate_transform);
        transform_group.Children.Add(R_rotation_transform);
        transform_group.Children.Add(SR_translate_transform);
        transform_group.Children.Add(SR_rotation_transform);

        for (var j = 0; j < SR_rings; j++)
        {
            for (var i = 0; i < R_rings; i++)
            {
                var start = transform_group.Transform(origin);

                R_rotation.Angle += R_angle_step;
                SR_rotation.Angle += SR_R_step_angle;

                var end = transform_group.Transform(origin);

                BuildPointRingHelper(Geometry.Positions, start, end - start, r, splits);
            }
        }


        BuildRingIndices(Geometry.TriangleIndices, SR_rings * R_rings, splits, true, true);

        return Geometry;
    }


    public static double[] BuildDeltas(int[] rings, double round_angle)
    {
        var deltas = new double[rings.Length];
        var last = rings[0] > 0 ? rings[0] : 1;
        deltas[0] = round_angle / last;
        for (int i = 1; i < rings.Length; i++)
        {
            last *= rings[i];
            deltas[i] = round_angle / last;
        }
        return deltas;
    }

    public static void IncrementAngles(AxisAngleRotation3D[] rotations, double[] deltas)
    {
        for (var i = 0; i < Math.Min(rotations.Length, deltas.Length); i++)
            rotations[i].Angle += deltas[i];
    }


    public static MeshGeometry3D BuildMultiSpringDonutGeometry3D(double GR = 1000, int GR_rings = 24, double SR = 200, int SR_rings = 24, double R = 50, int R_rings = 30, double r = 4, int splits = 15)
    {
        var Geometry = new MeshGeometry3D();

        var transform_group = new Transform3DGroup();

        var R_rotation = new AxisAngleRotation3D(y_axis, 0);
        var R_rotation_transform = new RotateTransform3D(R_rotation, origin);
        var R_translate_transform = new TranslateTransform3D(0, 0, R);

        var SR_rotation = new AxisAngleRotation3D(z_axis, 0);
        var SR_rotation_transform = new RotateTransform3D(SR_rotation, origin);
        var SR_translate_transform = new TranslateTransform3D(SR, 0, 0);

        var GR_rotation = new AxisAngleRotation3D(x_axis, 0);
        var GR_rotation_transform = new RotateTransform3D(GR_rotation, origin);
        var GR_translate_transform = new TranslateTransform3D(0, GR, 0);

        var final_rotation = new AxisAngleRotation3D(y_axis, 90);
        var final_rotation_transform = new RotateTransform3D(final_rotation, origin);

        AxisAngleRotation3D[] rotations = [R_rotation, SR_rotation, GR_rotation];

        //定位点的操作是先局部后整体先移动后转动
        transform_group.Children.Add(R_translate_transform);
        transform_group.Children.Add(R_rotation_transform);
        transform_group.Children.Add(SR_translate_transform);
        transform_group.Children.Add(SR_rotation_transform);
        transform_group.Children.Add(GR_translate_transform);
        transform_group.Children.Add(GR_rotation_transform);
        transform_group.Children.Add(GR_rotation_transform);

        transform_group.Children.Add(final_rotation_transform);


        int[] limits = [R_rings, SR_rings, GR_rings];
        var steps = new int[limits.Length];
        var deltas = BuildDeltas(limits, 360.0);
        var total = limits.Aggregate(1, (current, before) => current * before);

        for (var n = 0; n < total; n++)
        {
            var start = transform_group.Transform(origin);

            IncrementAngles(rotations, deltas);

            var end = transform_group.Transform(origin);

            //BuildCubeHelper(Geometry.Positions, Geometry.TriangleIndices, start, end - start, 1, 16);
            BuildPointRingHelper(Geometry.Positions, start, end - start, r, splits);
        }

        BuildRingIndices(Geometry.TriangleIndices, total, splits, true, false);

        return Geometry;
    }

    public static MeshGeometry3D BuildSuperMultiSpringDonutGeometry3D(double MR = 4000, int MR_rings = 24, double GR = 800, int GR_rings = 24, double SR = 200, int SR_rings = 24, double R = 50, int R_rings = 30, double r = 4, int splits = 15)
    {
        var Geometry = new MeshGeometry3D();

        var transform_group = new Transform3DGroup();

        var R_rotation = new AxisAngleRotation3D(y_axis, 0);
        var R_rotation_transform = new RotateTransform3D(R_rotation, origin);
        var R_translate_transform = new TranslateTransform3D(0, 0, R);

        var SR_rotation = new AxisAngleRotation3D(z_axis, 0);
        var SR_rotation_transform = new RotateTransform3D(SR_rotation, origin);
        var SR_translate_transform = new TranslateTransform3D(SR, 0, 0);

        var GR_rotation = new AxisAngleRotation3D(x_axis, 0);
        var GR_rotation_transform = new RotateTransform3D(GR_rotation, origin);
        var GR_translate_transform = new TranslateTransform3D(0, GR, 0);

        var MR_rotation = new AxisAngleRotation3D(y_axis, 0);
        var MR_rotation_transform = new RotateTransform3D(MR_rotation, origin);
        var MR_translate_transform = new TranslateTransform3D(0, 0, MR);



        var final_rotation = new AxisAngleRotation3D(x_axis, 90);
        var final_rotation_transform = new RotateTransform3D(final_rotation, origin);

        AxisAngleRotation3D[] rotations = [R_rotation, SR_rotation, GR_rotation, MR_rotation];

        //定位点的操作是先局部后整体先移动后转动
        transform_group.Children.Add(R_translate_transform);
        transform_group.Children.Add(R_rotation_transform);

        transform_group.Children.Add(SR_translate_transform);
        transform_group.Children.Add(SR_rotation_transform);

        transform_group.Children.Add(GR_translate_transform);
        transform_group.Children.Add(GR_rotation_transform);

        transform_group.Children.Add(MR_translate_transform);
        transform_group.Children.Add(MR_rotation_transform);


        transform_group.Children.Add(final_rotation_transform);


        int[] limits = [R_rings, SR_rings, GR_rings, MR_rings];
        var steps = new int[limits.Length];
        var deltas = BuildDeltas(limits, 360.0);
        var total = limits.Aggregate(1, (current, before) => current * before);

        for (var n = 0; n < total; n++)
        {
            var start = transform_group.Transform(origin);

            IncrementAngles(rotations, deltas);

            var end = transform_group.Transform(origin);

            //BuildCubeHelper(Geometry.Positions, Geometry.TriangleIndices, start, end - start, 1, 16);
            BuildPointRingHelper(Geometry.Positions, start, end - start, r, splits);
        }

        BuildRingIndices(Geometry.TriangleIndices, total, splits, true, false);

        return Geometry;
    }




}