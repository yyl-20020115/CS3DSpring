using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CS3DSpring;

public static class SpringBuilder
{
    public const double _2PI = 2.0 * Math.PI;
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

    public static ModelVisual3D Render(this MeshGeometry3D geometry, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                }
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

        BuildLayerIndices(Geometry.TriangleIndices, Segments, Segments, true);

        return Geometry;
    }

    public static void BuildLayerIndices(Int32Collection indices, int layers, int rings, bool close, bool flip = false)
    {
        for (int current_layer = 0; current_layer < (close ? layers : layers - 1); current_layer++)
        {
            int next_layer = (current_layer + 1) % layers;

            for (int current_point = 0; current_point < rings; current_point++)
            {
                int next_point = (current_point + 1) % rings;
                int p0 = current_layer * rings + current_point;
                int p1 = current_layer * rings + next_point;
                int p2 = next_layer * rings + current_point;
                int p3 = next_layer * rings + next_point;

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


        BuildLayerIndices(Geometry.TriangleIndices, Segments * Coils, Segments, false);

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

    public static MeshGeometry3D BuildFailedDonutSpringGeometry3D(double G = 240, double R = 20, double r = 5, int Coils = 120, int Segments = 120)
    {
        var Geometry = new MeshGeometry3D();

        var delta_yeta = 360.0 / Coils;
        var delta_theta = 360.0 / Segments;
        var delta_phi = 360.0 / Segments;

        var origin = new Point3D(0, 0, 0);
        var yeta_origin = origin;
        var x_axis = new Vector3D(1, 0, 0);
        var y_axis = new Vector3D(0, 1, 0);
        var z_axis = new Vector3D(0, 0, 1);

        var transform_group = new Transform3DGroup();

        var yeta_rotation = new AxisAngleRotation3D(z_axis, 0);
        var theta_rotation = new AxisAngleRotation3D(x_axis, 0);
        var phi_rotation = new AxisAngleRotation3D(y_axis, 0);

        var yeta_rotation_transform = new RotateTransform3D(yeta_rotation, origin);
        var yeta_translate_transform = new TranslateTransform3D(G, 0.0, 0.0);

        var theta_rotation_transform = new RotateTransform3D(theta_rotation, origin);
        var theta_translate_transform = new TranslateTransform3D(0, R, 0);

        var phi_rotation_transform = new RotateTransform3D(phi_rotation, origin);
        var phi_translate_transform = new TranslateTransform3D(r, 0, 0);

        transform_group.Children.Add(yeta_translate_transform);
        transform_group.Children.Add(yeta_rotation_transform);

        var yeta = 0.0;
        for (int c = 0; c < Coils; c++)
        {
            yeta_rotation.Angle = yeta;

            var theta_origin = transform_group.Value.Transform(origin);

            theta_rotation_transform.CenterX = theta_origin.X;
            theta_rotation_transform.CenterY = theta_origin.Y;
            theta_rotation_transform.CenterZ = theta_origin.Z;

            transform_group.Children.Add(theta_translate_transform);
            transform_group.Children.Add(theta_rotation_transform);

            var theta = 0.0;
            for (int i = 0; i < Segments; i++)
            {
                theta_rotation.Angle = theta;

                var phi_origin = transform_group.Value.Transform(origin);

                phi_rotation_transform.CenterX = phi_origin.X;
                phi_rotation_transform.CenterY = phi_origin.Y;
                phi_rotation_transform.CenterZ = phi_origin.Z;
                phi_rotation.Axis = Vector3D.CrossProduct(phi_origin - theta_origin, (Vector3D)theta_origin);
                transform_group.Children.Add(phi_translate_transform);
                transform_group.Children.Add(phi_rotation_transform);

                var phi = 0.0;
                for (int j = 0; j < Segments; j++)
                {
                    phi_rotation.Angle = phi;
                    Geometry.Positions.Add(transform_group.Value.Transform(origin));
                    phi += delta_phi;
                }
                transform_group.Children.Remove(phi_rotation_transform);
                transform_group.Children.Remove(phi_translate_transform);
                theta += delta_theta;
            }

            transform_group.Children.Remove(theta_rotation_transform);
            transform_group.Children.Remove(theta_translate_transform);

            yeta += delta_yeta;
        }
        BuildLayerIndices(Geometry.TriangleIndices, Segments * Coils, Segments, true);
        //Close curve
        return Geometry;
    }

    public static MeshGeometry3D BuildCylinderGeometry3D(Point3D from, Point3D to, double radius, int coils, int rings)
    {
        var Geometry = new MeshGeometry3D();

        BuildCylinder(Geometry.Positions, from, to, radius, coils, rings);
        //make double sides
        BuildLayerIndices(Geometry.TriangleIndices, coils, rings, false, true);
        BuildLayerIndices(Geometry.TriangleIndices, coils, rings, false, false);
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
    public static void BuildDiskEdgeHelper(Point3DCollection positions, Point3D from, Point3D to, double radius, int rings)
    {
        var z_axis = new Vector3D(0, 0, 1);
        var origin = new Point3D();

        var pointing = to - from;
        var axis = GetPerpendicularDirection(pointing, z_axis);
        var angle = GetIncludedAngle(z_axis, pointing).ToDegree();

        var transform_group = new Transform3DGroup();
        var rotation = new AxisAngleRotation3D(axis, angle);
        var rotation_transform = new RotateTransform3D(rotation, origin);
        var translate_transform = new TranslateTransform3D(from.X, from.Y, from.Z);

        transform_group.Children.Add(rotation_transform);
        transform_group.Children.Add(translate_transform);

        var angle_round = 0.0;
        var angle_delta = _2PI / rings;

        for (var i = 0; i < rings; i++)
        {
            var x = radius * Math.Cos(angle_round);
            var y = radius * Math.Sin(angle_round);
            var p = new Point3D(x, y, 0);
            positions.Add(transform_group.Transform(p));
            angle_round += angle_delta;
        }
    }


    public static void BuildDonutHelper(Point3DCollection positions, Point3D center, Point3D from, Point3D to, double radius, int coils, int rings)
    {
        var to_vector = to - center;
        var from_vector = from - center;
        var direction_vector = to - from;
        var axis = Vector3D.CrossProduct(from_vector, direction_vector); ;

        var main_height = (to - from).Length;
        var local_angle_round_delta = _2PI / rings;
        var delta_z = main_height / coils;

        var main_radius = (from - center).Length;
        var main_angle_delta = GetIncludedAngle(to_vector, from_vector) / coils;

        var transform_group = new Transform3DGroup();
        var rotation = new AxisAngleRotation3D(axis, 0);

        var rotation_transform = new RotateTransform3D(rotation, center);
        var translate_transform = new TranslateTransform3D((Vector3D)center);

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

    public static MeshGeometry3D BuildTestObjectGeometry3D()
    {
        var Geometry = new MeshGeometry3D();

        var r = 10;
        var R = 100.0;
        var rings = 360;
        var layers = 360;
        var angle = 0.0;
        var step_angle = _2PI / layers;


        for(var i = 0; i < layers; i++)
        {
            var x0 = R * Math.Cos(angle);
            var y0 = R * Math.Sin(angle);

            var x1 = R * Math.Cos(angle + step_angle);
            var y1 = R * Math.Sin(angle + step_angle);

            BuildDiskEdgeHelper(Geometry.Positions, new Point3D(x0, y0, 0), new Point3D(x1, y1, 0), r, rings);

            angle += step_angle;
        }

        BuildLayerIndices(Geometry.TriangleIndices, layers, rings, true, false);

        return Geometry;
    }
    public static MeshGeometry3D BuildDonutSpringsGeometry3D(
        (double Radius, int Coils, int Rings) Level, bool close = true, params (double Radius, int Coils, int Rings)[] Levels)
    {
        //当前层次的半径长度，缠绕多少圈，单圈分成多少段
        var Geometry = new MeshGeometry3D();

        List<(double Radius, int Coils, int Rings)> Rs = [Level];
        Rs.AddRange(Levels ?? []);
        var total_rigns = Rs.Select(r => r.Rings).Aggregate(1, (current, next) => current * next);
        PositionalVector origin
            = Rs.Count % 2 == 1
            ? new() { Position = new(0, 0, 0), Normal = new(0, 0, 1) }
            : new() { Position = new(Rs.Last().Radius, 0, 0), Normal = new(0, 1, 0) }
            ;

        var turns = Rs.Select(r => 360.0 / (r.Coils >= 1 ? r.Coils : 1)).ToArray();

        //var delta_yeta = 360.0 / Coils;
        //var delta_theta = 360.0 / Segments;
        //var delta_phi = 360.0 / Segments;
        //var yeta = 0.0;

        //var origin = new Point3D(0, 0, 0);
        //var yeta_origin = origin;
        //var x_axis = new Vector3D(1, 0, 0);
        //var y_axis = new Vector3D(0, 1, 0);
        //var z_axis = new Vector3D(0, 0, 1);

        //var transform_group = new Transform3DGroup();

        //var yeta_rotation = new AxisAngleRotation3D(z_axis, 0);
        //var theta_rotation = new AxisAngleRotation3D(x_axis, 0);
        //var phi_rotation = new AxisAngleRotation3D(y_axis, 0);

        //var yeta_rotation_transform = new RotateTransform3D(yeta_rotation, origin);
        //var yeta_translate_transform = new TranslateTransform3D(G, 0.0, 0.0);

        //var theta_rotation_transform = new RotateTransform3D(theta_rotation, origin);
        //var theta_translate_transform = new TranslateTransform3D(0, R, 0);

        //var phi_rotation_transform = new RotateTransform3D(phi_rotation, origin);
        //var phi_translate_transform = new TranslateTransform3D(r, 0, 0);

        //transform_group.Children.Add(yeta_translate_transform);
        //transform_group.Children.Add(yeta_rotation_transform);

        //yeta = 0.0;
        //for (int c = 0; c < Coils; c++)
        //{
        //    yeta_rotation.Angle = yeta;

        //    var theta_origin = transform_group.Value.Transform(origin);

        //    theta_rotation_transform.CenterX = theta_origin.X;
        //    theta_rotation_transform.CenterY = theta_origin.Y;
        //    theta_rotation_transform.CenterZ = theta_origin.Z;

        //    transform_group.Children.Add(theta_translate_transform);
        //    transform_group.Children.Add(theta_rotation_transform);

        //    var theta = 0.0;
        //    for (int i = 0; i < Segments; i++)
        //    {
        //        theta_rotation.Angle = theta;

        //        var phi_origin = transform_group.Value.Transform(origin);

        //        phi_rotation_transform.CenterX = phi_origin.X;
        //        phi_rotation_transform.CenterY = phi_origin.Y;
        //        phi_rotation_transform.CenterZ = phi_origin.Z;
        //        phi_rotation.Axis = Vector3D.CrossProduct(phi_origin - theta_origin, (Vector3D)theta_origin);
        //        transform_group.Children.Add(phi_translate_transform);
        //        transform_group.Children.Add(phi_rotation_transform);

        //        var phi = 0.0;
        //        for (int j = 0; j < Segments; j++)
        //        {
        //            phi_rotation.Angle = phi;

        //            Positions.Add(transform_group.Value.Transform(origin));

        //            phi += delta_phi;
        //        }
        //        transform_group.Children.Remove(phi_rotation_transform);
        //        transform_group.Children.Remove(phi_translate_transform);
        //        theta += delta_theta;
        //    }
        //    transform_group.Children.Remove(theta_rotation_transform);
        //    transform_group.Children.Remove(theta_translate_transform);
        //    yeta += delta_yeta;
        //}

        //Close curve
        int layer_rings = Level.Rings;
        int total_layers = total_rigns / Level.Rings;

        BuildLayerIndices(Geometry.TriangleIndices, layer_rings, total_layers, true);

        return Geometry;
    }
}