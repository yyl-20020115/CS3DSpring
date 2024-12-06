using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Effects;
using System.Collections.Generic;

namespace CS3DSpring;


public partial class MainWindow : Window
{
    //鼠标灵敏度调节
    private const double MouseDeltaFactor = 2;

    private readonly PerspectiveCamera Camera;
    public MainWindow()
    {

        InitializeComponent();
        Camera = new PerspectiveCamera
        {
            Position = new(0, 0, 1500),
            LookDirection = new(0, 0, -1),
            FieldOfView = 1000
        };
        viewPort.Camera = Camera;

        this.SpringDonutWithCones.IsChecked = true;
    }

    public void SetModels(params List<ModelVisual3D> WorldModels)
    {
        WorldModels.Add(SpringBuilder.BuildCubeGeometry3D(new Point3D(), new Vector3D(1, 0, 0), 1, 24000).Render(Camera.Position, Camera.LookDirection, Brushes.Indigo));
        WorldModels.Add(SpringBuilder.BuildCubeGeometry3D(new Point3D(), new Vector3D(0, 1, 0), 1, 24000).Render(Camera.Position, Camera.LookDirection, Brushes.Lime));
        WorldModels.Add(SpringBuilder.BuildCubeGeometry3D(new Point3D(), new Vector3D(0, 0, 1), 1, 24000).Render(Camera.Position, Camera.LookDirection, Brushes.Orchid));

        viewPort.Children.Clear();
        
        foreach (var model in WorldModels)
        {
            viewPort.Children.Add(model);
        }

        viewPort.MouseEnter += Vp_MouseEnter;
        viewPort.MouseLeave += Vp_MouseLeave;
    }

    private void Vp_MouseLeave(object sender, MouseEventArgs e)
    {
        viewPort.Effect = null;
    }

    private void Vp_MouseEnter(object sender, MouseEventArgs e)
    {
        var BlurRadius = new DropShadowEffect
        {
            BlurRadius = 20,
            Color = System.Windows.Media.Colors.Yellow,
            Direction = 0,
            Opacity = 1,
            ShadowDepth = 0
        };
        viewPort.Effect = BlurRadius;
    }


    public HitTestResultBehavior HTResult(HitTestResult rawresult)
    {
        if (rawresult is RayHitTestResult rayResult)
        {
            //RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
            if (rayResult is not null)
            {
                //GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                var visual3D = rawresult.VisualHit as ModelVisual3D;

                //do something

            }
        }

        return HitTestResultBehavior.Continue;
    }

    //鼠标位置
    System.Windows.Point mouseLastPosition;

    private void Vp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        mouseLastPosition = e.GetPosition(this);
        //RayHitTestParameters hitParams = new RayHitTestParameters(myPCamera.Position, myPCamera.LookDirection);
        //VisualTreeHelper.HitTest(vp.Children[0], null, ResultCallback, hitParams);

        //下面是进行点击触发检测，可忽略，注释
        var testpoint3D = new Point3D(mouseLastPosition.X, mouseLastPosition.Y, 0);
        var testdirection = new Vector3D(mouseLastPosition.X, mouseLastPosition.Y, 100);
        var pointparams = new PointHitTestParameters(mouseLastPosition);
        var rayparams = new RayHitTestParameters(testpoint3D, testdirection);

        //test for a result in the Viewport3D
        VisualTreeHelper.HitTest(viewPort, null, HTResult, pointparams);
    }

    //鼠标旋转
    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (Mouse.LeftButton == MouseButtonState.Pressed)//如果按下鼠标左键
        {
            var newMousePosition = e.GetPosition(this);

            if (mouseLastPosition.X != newMousePosition.X)
            {
                //进行水平旋转
                HorizontalTransform(mouseLastPosition.X < newMousePosition.X, MouseDeltaFactor);//水平变换
            }

            if (mouseLastPosition.Y != newMousePosition.Y)// change position in the horizontal direction
            {
                //进行垂直旋转
                VerticalTransform(mouseLastPosition.Y > newMousePosition.Y, MouseDeltaFactor);//垂直变换 
            }

            mouseLastPosition = newMousePosition;
        }
    }

    //鼠标滚轮缩放
    private void VP_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        double scaleFactor = 240;
        //120 near ,   -120 far
        //System.Diagnostics.Debug.WriteLine(e.Delta.ToString());
        var currentPosition = Camera.Position;
        var lookDirection = Camera.LookDirection;
        lookDirection.Normalize();
        lookDirection *= scaleFactor;

        if (e.Delta > 0)//getting near
        {
            //myPCamera.FieldOfView /= 1.2;
            currentPosition += lookDirection;
        }
        if (e.Delta < 0)//getting far
        {
            //myPCamera.FieldOfView *= 1.2;
            currentPosition -= lookDirection;
        }
        Camera.Position = currentPosition;
    }

    // 垂直变换
    private void VerticalTransform(bool upDown, double angleDeltaFactor)
    {
        Vector3D postion = new(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
        Vector3D rotateAxis = Vector3D.CrossProduct(postion, Camera.UpDirection);
        RotateTransform3D rt3d = new();
        AxisAngleRotation3D rotate = new(rotateAxis, angleDeltaFactor * (upDown ? 1 : -1));
        rt3d.Rotation = rotate;
        Matrix3D matrix = rt3d.Value;
        Point3D newPostition = matrix.Transform(Camera.Position);
        Camera.Position = newPostition;
        Camera.LookDirection = new Vector3D(-newPostition.X, -newPostition.Y, -newPostition.Z);

        //update the up direction
        //Vector3D newUpDirection = Vector3D.CrossProduct(myPCamera.LookDirection, rotateAxis);
        //newUpDirection.Normalize();
        //myPCamera.UpDirection = newUpDirection;
    }
    // 水平变换：
    private void HorizontalTransform(bool leftRight, double angleDeltaFactor)
    {
        Vector3D postion = new(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
        Vector3D rotateAxis = Camera.UpDirection;
        RotateTransform3D rt3d = new();
        AxisAngleRotation3D rotate = new(rotateAxis, angleDeltaFactor * (leftRight ? 1 : -1));
        rt3d.Rotation = rotate;
        Matrix3D matrix = rt3d.Value;
        Point3D newPostition = matrix.Transform(Camera.Position);
        Camera.Position = newPostition;
        Camera.LookDirection = new Vector3D(-newPostition.X, -newPostition.Y, -newPostition.Z);
    }
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        var p = Camera.Position;
        var any = false;
        var offset = MouseDeltaFactor;
        switch (e.Key)
        {
            case Key.Left:
                p.Offset(offset, 0, 0);
                any = true;
                break;
            case Key.Right:
                p.Offset(-offset, 0, 0);
                any = true;
                break;
            case Key.Up:
                p.Offset(0, -offset, 0);
                any = true;
                break;
            case Key.Down:
                p.Offset(0, offset, 0);
                any = true;
                break;
        }
        if (any)
            Camera.Position = p;
    }

    private void RadioButton_Click(object sender, RoutedEventArgs e)
    {

        if (sender == this.Cylinder)
        {
            SetModels(
                SpringBuilder.BuildCylinderGeometry3D(new Point3D(), new Point3D(0, 0, 100), 100, 10, 360).Render(Camera.Position, Camera.LookDirection, Brushes.Aqua)
            );
        }
        else if (sender == this.Donut)
        {
            SetModels(
                SpringBuilder.BuildDonutGeometry3D(new Point3D()).Render(Camera.Position, Camera.LookDirection, Brushes.Green)
            );
        }
        else if (sender == this.Spring)
        {
            SetModels(
                SpringBuilder.BuildSpringGeometry3D(new Point3D()).Render(Camera.Position, Camera.LookDirection, Brushes.Brown)
            );
        }
        else if (sender == this.SpringDonut)
        {
            SetModels(
                SpringBuilder.BuildSpringDonutGeometry3D(new Point3D()).Render(Camera.Position, Camera.LookDirection, Brushes.Chocolate)
                );
        }
        else if (sender == this.SpringDonutWithCones)
        {
            SetModels(
                SpringBuilder.BuildSpringDonutWithConesGeometry3D(new Point3D()).Render(Camera.Position, Camera.LookDirection, Brushes.CadetBlue)
            );
        }
        else if (sender == this.MultiSpringDonut)
        {
            SetModels(
                SpringBuilder.BuildMultiSpringDonutGeometry3D(new Point3D()).Render(Camera.Position, Camera.LookDirection, Brushes.Cyan)
            );
        }
        else if (sender == this.SuperMultiSpringDonut)
        {
            SetModels(
                SpringBuilder.BuildUnlimitedSpringDonutGeometry3D(new Point3D(), 64, 64).Render(Camera.Position, Camera.LookDirection, Brushes.Red),
                SpringBuilder.BuildUnlimitedSpringDonutGeometry3D(new Point3D(), 8, 30, (200, 64)).Render(Camera.Position, Camera.LookDirection, Brushes.Orange),
                SpringBuilder.BuildUnlimitedSpringDonutGeometry3D(new Point3D(), 4, 64, (50, 32), (400, 64)).Render(Camera.Position, Camera.LookDirection, Brushes.Blue),
                SpringBuilder.BuildUnlimitedSpringDonutGeometry3D(new Point3D(), 4, 30, (50, 48), (100, 36), (800, 24)).Render(Camera.Position, Camera.LookDirection, Brushes.Green)
            );
        }else if(sender == this.FinalSpringDonut)
        {
            SetModels(
                SpringBuilder.BuildUnlimitedSpringDonutGeometry3D(new Point3D(), 2, 15, (50, 24), (100, 12), (200, 18), (1600, 18)).Render(Camera.Position, Camera.LookDirection, Brushes.DarkCyan)
            );
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Maximized;
        RadioButton_Click(this.SpringDonutWithCones, new RoutedEventArgs());
    }
}