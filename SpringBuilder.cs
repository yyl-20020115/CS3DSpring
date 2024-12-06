using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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


    public static double ToDegree(this double radian)
        => radian / Math.PI * 180.0;
    public static double GetIncludedAngle(Vector3D from, Vector3D to)
    {
        from.Normalize();
        to.Normalize();
        return Math.Acos(Vector3D.DotProduct(to, from));
    }

    public static ModelVisual3D Render(this MeshGeometry3D geometry, Point3D? position = null, Vector3D? direction = null, Brush? brush = null, Color? light = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? System.Windows.Media.Colors.White,
                },
                //new SpotLight(light ?? Colors.White, position??new Point3D(),direction??new Vector3D(),10,5)
            };
        var material = new DiffuseMaterial(brush) { AmbientColor = light ?? System.Windows.Media.Colors.White };
        collection.Add(new GeometryModel3D(geometry, material));
        group!.Children = collection;
        return model;

    }

    public static void BuildDonut(Point3DCollection positions, Point3D center, double R, double r, uint splits)
    {
        var delta_angle = _2PI / splits;
        var theta = 0.0;
        for (uint i = 0; i < splits; i++)
        {
            var phi = 0.0;
            for (uint j = 0; j < splits; j++)
            {
                positions.Add(new
                    (center.X + (R + r * Math.Cos(phi)) * Math.Cos(theta),
                     center.Y + (R + r * Math.Cos(phi)) * Math.Sin(theta),
                     center.Z + r * Math.Sin(phi)
                    ));
                phi += delta_angle;
            }
            theta += delta_angle;
        }
    }
    public static MeshGeometry3D BuildDonutGeometry3D(Point3D center, double R = 50, uint splits = 360, double r = 10)
    {
        var Geometry = new MeshGeometry3D();

        BuildDonut(Geometry.Positions, center, R, r, splits);
        BuildRingIndices(Geometry.TriangleIndices, 0, splits, splits, true);

        return Geometry;
    }

    public static void BuildRingIndices(Int32Collection indices, uint _base, uint rings, uint splits, bool close, bool flip = false)
    {
        for (uint current_ring = 0; current_ring < (close ? rings : rings - 1); current_ring++)
        {
            uint next_ring = (current_ring + 1) % rings;
            for (uint current_point = 0; current_point < splits; current_point++)
            {
                uint next_point = (current_point + 1) % splits;
                uint p0 = _base + current_ring * splits + current_point;
                uint p1 = _base + current_ring * splits + next_point;
                uint p2 = _base + next_ring * splits + current_point;
                uint p3 = _base + next_ring * splits + next_point;
                indices.Add((int)p1);
                if (flip)
                {
                    indices.Add((int)p2);
                    indices.Add((int)p0);
                }
                else
                {
                    indices.Add((int)p0);
                    indices.Add((int)p2);
                }
                indices.Add((int)p2);
                if (flip)
                {
                    indices.Add((int)p1);
                    indices.Add((int)p3);
                }
                else
                {
                    indices.Add((int)p3);
                    indices.Add((int)p1);
                }
            }
        }
    }

    public static MeshGeometry3D BuildSpringGeometry3D(Point3D center, uint Coils = 4, double R = 50, uint splits = 360, double r = 10)
    {
        var Geometry = new MeshGeometry3D();

        double delta_angle = _2PI / splits;
        double theta = 0.0;
        double delta_z = r * Coils / (splits * splits);
        double z = 0.0;

        for (uint c = 0; c < Coils; c++)
        {
            for (uint i = 0; i < splits; i++)
            {
                var phi = 0.0;
                for (uint j = 0; j < splits; j++)
                {
                    Geometry.Positions.Add(new
                        (center.X + (R + r * Math.Cos(phi)) * Math.Cos(theta),
                         center.Y + (R + r * Math.Cos(phi)) * Math.Sin(theta),
                         center.Z + z + r * Math.Sin(phi)
                        ));
                    z += delta_z;
                    phi += delta_angle;
                }
                theta += delta_angle;
            }
        }


        BuildRingIndices(Geometry.TriangleIndices, 0, splits * Coils, splits, false);
        BuildCoverIndices(Geometry.Positions, Geometry.TriangleIndices, splits, R, z);
        return Geometry;
    }
    public static void BuildCoverIndices(Point3DCollection positions, Int32Collection indices, uint splits, double R, double z)
    {
        positions.Add(new Point3D(R, 0, 0));
        positions.Add(new Point3D(R, 0, z));

        uint first_center = (uint)(positions.Count - 2);
        uint last_center = (uint)(positions.Count - 1);
        for (uint current_point = 0; current_point < splits - 1; current_point++)
        {
            uint next_point = (current_point + 1) % splits;

            indices.Add((int)current_point);
            indices.Add((int)next_point);
            indices.Add((int)first_center);
        }
        for (uint current_point = (uint)(positions.Count - 4); current_point >= (uint)(positions.Count - 3 - splits); current_point--)
        {
            uint next_point = current_point + 1;

            indices.Add((int)next_point);
            indices.Add((int)current_point);
            indices.Add((int)last_center);
        }
    }


    public static void BuildSphare(Point3DCollection positions, Int32Collection indices, Point3D center, double radius, uint stacks = 64, uint slices = 64, Vector3DCollection? normals = null, PointCollection? textures = null)
    {
        uint _base = (uint)positions.Count;
        // Fill the vertices, normals, and textures collections.
        for (uint stack = 0; stack <= stacks; stack++)
        {
            double phi = Math.PI / 2 - stack * Math.PI / stacks;
            double y = radius * Math.Sin(phi);
            double scale = -radius * Math.Cos(phi);

            for (uint slice = 0; slice <= slices; slice++)
            {
                double theta = slice * 2 * Math.PI / slices;
                double x = scale * Math.Sin(theta);
                double z = scale * Math.Cos(theta);

                var normal = new Vector3D(x, y, z);
                normals?.Add(normal);
                positions.Add(normal + center);
                textures?.Add(new Point((double)slice / slices,
                                      (double)stack / stacks));
            }
        }


        // Fill the indices collection.
        for (uint stack = 0; stack < stacks; stack++)
        {
            uint top = _base + (stack + 0) * (slices + 1);
            uint bot = _base + (stack + 1) * (slices + 1);

            for (uint slice = 0; slice < slices; slice++)
            {
                if (stack != 0)
                {
                    indices.Add((int)(top + slice));
                    indices.Add((int)(bot + slice));
                    indices.Add((int)(top + slice + 1));
                }

                if (stack != stacks - 1)
                {
                    indices.Add((int)(top + slice + 1));
                    indices.Add((int)(bot + slice));
                    indices.Add((int)(bot + slice + 1));
                }
            }
        }
    }
    public static MeshGeometry3D BuildCylinderGeometry3D(Point3D from, Point3D to, double radius, uint coils, uint splits)
    {
        var Geometry = new MeshGeometry3D();

        BuildCylinder(Geometry.Positions, from, to, radius, coils, splits);
        //make double sides
        BuildRingIndices(Geometry.TriangleIndices, 0, coils, splits, false, true);
        BuildRingIndices(Geometry.TriangleIndices, 0, coils, splits, false, false);


        return Geometry;
    }

    public static void BuildCylinder(Point3DCollection positions, Point3D from, Point3D to, double radius, uint coils, uint splits)
    {
        var axis = to - from;

        var main_height = (to - from).Length;
        var local_angle_round_delta = _2PI / splits;
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
            for (var i = 0; i < splits; i++)
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
    public static void BuildPointRingHelper(Point3DCollection positions, Point3D from, Vector3D pointing, double radius, uint splits)
    {
        pointing.Normalize();
        var axis = GetPerpendicularDirection(z_axis, pointing);
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
    public static MeshGeometry3D BuildCubeGeometry3D(Point3D center, Vector3D pointing, double radius, double length)
    {
        var Geometry = new MeshGeometry3D();
        BuildCubeHelper(Geometry.Positions, Geometry.TriangleIndices, center, pointing, radius, length / radius);
        return Geometry;
    }
    public static void BuildCubeHelper(Point3DCollection positions, Int32Collection indices, Point3D from, Vector3D pointing, double radius = 1, double ratio = 8)
    {
        pointing.Normalize();
        var axis = GetPerpendicularDirection(z_axis, pointing);
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
        var _base = positions.Count;

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

        foreach (var index in _indices)
        {
            indices.Add(index + _base);
        }
    }

    public static void BuildConeHelper(Point3DCollection positions, Int32Collection indices, Point3D from, Vector3D pointing, double radius = 1, double ratio = 8, uint splits = 64)
    {
        pointing.Normalize();

        var axis = GetPerpendicularDirection(z_axis, pointing);
        var angle = GetIncludedAngle(z_axis, pointing).ToDegree();

        var transform_group = new Transform3DGroup();

        var rotation = new AxisAngleRotation3D(axis, angle);
        var rotation_transform = new RotateTransform3D(rotation, origin);
        var translate_transform = new TranslateTransform3D(from.X, from.Y, from.Z);

        transform_group.Children.Add(rotation_transform);
        transform_group.Children.Add(translate_transform);

        var _base = positions.Count;
        positions.Add(transform_group.Transform(new Point3D(0.0, 0.0, radius * ratio)));
        positions.Add(transform_group.Transform(new Point3D(0.0, 0.0, 0)));

        var spin_angle_delta = _2PI / splits;
        var spin_angle = 0.0;
        for (uint i = 0; i <= splits; i++)
        {
            var point = new Point3D(radius * Math.Cos(spin_angle), radius * Math.Sin(spin_angle), 0.0);
            positions.Add(transform_group.Transform(point));
            spin_angle += spin_angle_delta;
            indices.Add(_base + 0);
            indices.Add((int)(_base + i + 0));
            indices.Add((int)(_base + i + 1));
            indices.Add(_base + 1);
            indices.Add((int)(_base + i + 1));
            indices.Add((int)(_base + i + 0));
        }
    }

    public static MeshGeometry3D BuildSpringDonutWithCubesGeometry3D(Point3D center, double SR = 500, uint SR_rings = 24, double R = 200, uint R_rings = 30, double r = 2, double ratio = 16)
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
        transform_group.Children.Add(new TranslateTransform3D((Vector3D)center));

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

    public static MeshGeometry3D BuildSpringDonutWithConesGeometry3D(Point3D center, double SR = 500, uint SR_rings = 24, double R = 200, uint R_rings = 30, double r = 2, double ratio = 16)
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
        transform_group.Children.Add(new TranslateTransform3D((Vector3D)center));

        for (var j = 0; j < SR_rings; j++)
        {
            for (var i = 0; i < R_rings; i++)
            {
                var start = transform_group.Transform(origin);

                R_rotation.Angle += R_angle_step;
                SR_rotation.Angle += SR_R_step_angle;

                var end = transform_group.Transform(origin);

                BuildConeHelper(Geometry.Positions, Geometry.TriangleIndices, start, end - start, r, ratio);
            }
        }

        return Geometry;
    }


    public static MeshGeometry3D BuildSpringDonutGeometry3D(Point3D center, double SR = 500, uint SR_rings = 48, double R = 100, uint R_rings = 120, double r = 10, uint splits = 30)
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
        transform_group.Children.Add(new TranslateTransform3D((Vector3D)center));

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


        BuildRingIndices(Geometry.TriangleIndices, 0, SR_rings * R_rings, splits, true, true);

        return Geometry;
    }


    public static double[] BuildDeltas(uint[] rings, double round_angle)
    {
        var deltas = new double[rings.Length];
        var last = rings[0] > 0 ? rings[0] : 1;
        deltas[0] = round_angle / last;
        for (uint i = 1; i < rings.Length; i++)
        {
            last *= rings[i];
            deltas[i] = round_angle / last;
        }
        return deltas;
    }

    public static void IncrementAngles(List<AxisAngleRotation3D> rotations, double[] deltas)
    {
        for (var i = 0; i < Math.Min(rotations.Count, deltas.Length); i++)
            rotations[i].Angle += deltas[i];
    }


    public static MeshGeometry3D BuildMultiSpringDonutGeometry3D(Point3D center, double GR = 1000, uint GR_rings = 48, double SR = 200, uint SR_rings = 24, double R = 50, uint R_rings = 30, double r = 8, uint splits = 15)
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

        List<AxisAngleRotation3D> rotations = [R_rotation, SR_rotation, GR_rotation];

        //定位点的操作是先局部后整体先移动后转动
        transform_group.Children.Add(R_translate_transform);
        transform_group.Children.Add(R_rotation_transform);
        transform_group.Children.Add(SR_translate_transform);
        transform_group.Children.Add(SR_rotation_transform);
        transform_group.Children.Add(GR_translate_transform);
        transform_group.Children.Add(GR_rotation_transform);
        transform_group.Children.Add(GR_rotation_transform);

        transform_group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(y_axis, 90), origin));
        transform_group.Children.Add(new TranslateTransform3D((Vector3D)center));


        uint[] limits = [R_rings, SR_rings, GR_rings];
        var steps = new uint[limits.Length];
        var deltas = BuildDeltas(limits, 360.0);
        var total = limits.Aggregate(1U, (current, before) => current * before);

        for (var n = 0; n < total; n++)
        {
            var start = transform_group.Transform(origin);

            IncrementAngles(rotations, deltas);

            var end = transform_group.Transform(origin);

            //BuildCubeHelper(Geometry.Positions, Geometry.TriangleIndices, start, end - start, 1, 16);
            BuildPointRingHelper(Geometry.Positions, start, end - start, r, splits);
        }

        BuildRingIndices(Geometry.TriangleIndices, 0, total, splits, true, false);

        return Geometry;
    }
    public static readonly Vector3D[] axes = [y_axis, z_axis, x_axis];
    public static readonly Vector3D[] flip_axes = [y_axis, z_axis, x_axis, y_axis];
    public static Vector3D GetRotationAxis(uint i, bool flip)
        => flip ? flip_axes[i % 4] : axes[i % 3];

    public static Vector3D GetOffset(uint i, double r, bool flip)
    {
        if (flip)
        {
            return (i % 4) switch
            {
                0 => new Vector3D(0, 0, r),
                1 => new Vector3D(r, 0, 0),
                2 => new Vector3D(0, r, 0),
                3 => new Vector3D(0, 0, r),
                _ => new Vector3D()
            };

        }
        else
        {
            return (i % 3) switch
            {
                0 => new Vector3D(0, 0, r),
                1 => new Vector3D(r, 0, 0),
                2 => new Vector3D(0, r, 0),
                _ => new Vector3D()
            };
        }
    }

    public static Transform3D[] GenerateTransforms(uint i, double r, bool flip, out AxisAngleRotation3D rotation)
        => [new TranslateTransform3D(GetOffset(i, r,flip)),
            new RotateTransform3D(rotation = new AxisAngleRotation3D(GetRotationAxis(i,flip), 0), origin)];

    public static MeshGeometry3D BuildUnlimitedSpringDonutGeometry3D(Point3D center, double r = 4, uint splits = 64, params (double radius, uint rings)[] Rs)
    {
        var Geometry = new MeshGeometry3D();

        if (Rs.Length == 0)
        {
            BuildSphare(Geometry.Positions, Geometry.TriangleIndices, center, r, splits, splits);
            return Geometry;
        }

        var transform_group = new Transform3DGroup();

        var rotations = new List<AxisAngleRotation3D>();

        for (uint i = 0; i < Rs.Length; i++)
        {
            var transforms = GenerateTransforms(i, Rs[i].radius, Rs.Length >= 4, out var rotation);

            rotations.Add(rotation);

            transform_group.Children.Add(transforms[0]);
            transform_group.Children.Add(transforms[1]);
        }

        transform_group.Children.Add(new TranslateTransform3D((Vector3D)center));

        var limits = Rs.Select(r => r.rings).ToArray();

        var steps = new uint[limits.Length];
        var deltas = BuildDeltas(limits, 360.0);
        var total = limits.Aggregate(1U, (current, before) => current * before);

        for (var n = 0; n < total; n++)
        {
            var start = transform_group.Transform(origin);

            IncrementAngles(rotations, deltas);

            var end = transform_group.Transform(origin);

            BuildPointRingHelper(Geometry.Positions, start, end - start, r, splits);
        }

        BuildRingIndices(Geometry.TriangleIndices, 0, total, splits, true, false);

        return Geometry;
    }

    public static MeshGeometry3D BuildDepthSpringDonutGeometry3D(Point3D center, double R, uint depth, uint max_rings, double r = 4, uint splits = 12)
    {
        //0-4可以绘制，大于4的限制在4之内，
        depth %= 4;
        var r0 = Math.Pow(R, 1.0 / 4.0);
        var d0 = max_rings >> 2;
        List<(double, uint)> Rs = [];
        for (uint i = 0; i < depth; i++)
        {
            Rs.Add((Math.Pow(r0, i + 1), d0 * (4 - i)));
        }
        return BuildUnlimitedSpringDonutGeometry3D(center, r, splits, [.. Rs]);
    }

}