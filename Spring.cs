using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CS3DSpring;

public static class Spring
{
    public const double _2PI = 2.0 * Math.PI;

    public static ModelVisual3D BuildDonutModelVisual3D(
        Point3D Center, double R = 50, int Segments = 360, double r = 10, Color? light = null, Brush? brush = null)
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


        var Positions = new Point3DCollection();
        var Indices = new Int32Collection();
        var Textures = new PointCollection();
        var geometry = new MeshGeometry3D();
        var material = new DiffuseMaterial(brush);

        double delta_angle = _2PI / Segments;
        double theta = 0.0;
        for (int i = 0; i < Segments; i++)
        {
            var phi = 0.0;
            for (int j = 0; j < Segments; j++)
            {
                Positions.Add(new 
                    ((R + r * Math.Cos(phi)) * Math.Cos(theta),
                     (R + r * Math.Cos(phi)) * Math.Sin(theta),
                     r * Math.Sin(phi)
                    ));
                phi += delta_angle;
            }
            theta += delta_angle;
        }

        for (int current_layer = 0; current_layer < Segments; current_layer++)
        {
            int next_layer = (current_layer + 1) % Segments;

            for(int current_point = 0; current_point < Segments; current_point++)
            {
                int next_point = (current_point + 1) % Segments;
                int p0 = current_layer * Segments + current_point;
                int p1 = current_layer * Segments + next_point;
                int p2 = next_layer * Segments + current_point;
                int p3 = next_layer * Segments + next_point;

                Indices.Add(p1);
                Indices.Add(p0);
                Indices.Add(p2);

                Indices.Add(p2);
                Indices.Add(p3);
                Indices.Add(p1);

            }
        }

        geometry.Positions = Positions;
        geometry.TriangleIndices = Indices;
        geometry.TextureCoordinates = Textures;
        collection.Add(new GeometryModel3D(geometry, material));
        group.Children = collection;
        return model;
    }

    /// <summary>
    /// 绘制重弹簧
    /// </summary>
    /// <param name="SpringLength">弹簧的长度</param>
    /// <param name="Radius">弹簧的半径</param>
    /// <param name="Coils">弹簧的圈数</param>
    /// <param name="Rings">弹簧每圈的段数</param>
    /// <param name="Sides">弹簧每段的侧面数</param>
    /// <param name="TubeRadiusRatio">弹簧的段半径</param>
    public static ModelVisual3D BuildHeavySpringModelVisual3D(
        Point3D Start, Point3D End, double Radius = 50, double Coils = 10, int Rings = 60, int Sides = 18, double TubeRadiusRatio = 0.1, int Depth = 1, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Blue;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                }
            };


        var Positions = new Point3DCollection();
        var Indices = new Int32Collection();
        var Textures = new PointCollection();
        var geometry = new MeshGeometry3D();
        var material = new DiffuseMaterial(brush);

        BuildHeavySpring(Positions, Indices, Textures, Start, End, Radius, Coils, Rings, Sides, TubeRadiusRatio, Depth);

        geometry.Positions = Positions;
        geometry.TriangleIndices = Indices;
        geometry.TextureCoordinates = Textures;
        collection.Add(new GeometryModel3D(geometry, material));
        group.Children = collection;
        return model;
    }
    public static void BuildHeavySpring(
        Point3DCollection Positions,
        Int32Collection Indices,
        PointCollection Textures,
        Point3D Start, Point3D End, double Radius = 50, double Coils = 10, int Rings = 60, int Sides = 18, double TubeRadiusRatio = 0.1, int Depth = 1)
    {
        double TubeRadius = Radius * TubeRadiusRatio;
        double sideDelta = _2PI / Sides;
        double ringDelta = _2PI / Rings;
        double theta = 0.0;
        double cosTheta0 = 1.0;
        double sinTheta0 = 0.0;
        double z = 0.0;
        double SpringLength = (Start - End).Length;

        double RingHeight = SpringLength / Coils / Rings; //一段的高=总长/圈数/每圈的段数

        for (int i = 0; i < Coils; i++)
        {
            for (int j = 0; j < Rings; j++)
            {
                double theta1 = theta + ringDelta;
                double cosTheta1 = Math.Cos(theta1);
                double sinTheta1 = Math.Sin(theta1);

                double phi = 0.0;
                for (int k = 0; k <= Sides; k++)
                {
                    phi += sideDelta;
                    double cosPhi = Math.Cos(phi);
                    double sinPhi = Math.Sin(phi);
                    double dist = Radius + (TubeRadius * cosPhi);
                    var p0 = new Point3D(cosTheta0 * dist, sinTheta0 * dist, z + TubeRadius * sinPhi);
                    var p1 = new Point3D(cosTheta1 * dist, sinTheta1 * dist, z + TubeRadius * sinPhi + RingHeight);

                    if (Depth > 1)
                    {
                        BuildHeavySpring(Positions, Indices, Textures, p0, p1, TubeRadius, Coils, Rings, Sides, TubeRadiusRatio, Depth - 1);
                    }
                    else
                    {
                        //Draw it!
                    }

                }
                theta = theta1;
                cosTheta0 = cosTheta1;
                sinTheta0 = sinTheta1;

                z += RingHeight;
            }
        }
    }
}
