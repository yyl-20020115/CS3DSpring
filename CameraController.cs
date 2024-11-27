using System;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CSWall;

public class CameraController
{

    // 每次转换的的最小值
    public const double cmDR = 0.1;
    public const double cmDTheta = Math.PI / 30;

    //相机
    public PerspectiveCamera cm = null;

    // 传入主窗口的动作
    private UIElement mainWindow = null;


    // 相机位置和方向
    public Point3D cmPosition = new (0, 0, 0);
    public double cmTheta = Math.PI * 1.3;

    // Constructor.
    public CameraController(PerspectiveCamera camera, Viewport3D viewport,
        UIElement mainWindow)
    {
        cm = camera;
        viewport.Camera = cm;
        this.mainWindow = mainWindow;
        this.mainWindow.PreviewKeyDown += MainWindow_KeyDown;

        this.mainWindow.PreviewMouseWheel += MainWindow_PreviewMouseWheel;

        this.mainWindow.MouseLeftButtonDown += mainWindow_LeftDown;

        PositionCameraMouse();
    }



    // 将角度转为向量
    protected Vector3D AngleToVector(double angle, double length)
    {
        return new Vector3D(
            length * Math.Cos(angle), 0, length * Math.Sin(angle));
    }

    protected void MoveLR(bool isLeft=true)
    {
        Vector3D v = AngleToVector(cmTheta, cmDR);
        if (isLeft)
            cmPosition += new Vector3D(v.Z, 0, -v.X);
        else
            cmPosition += new Vector3D(-v.Z, 0, v.X);
    }

    //向上或者向下移动
    protected void MoveUD(bool isUp = true)
    {
        Vector3D v = AngleToVector(cmTheta, cmDR);
        if (isUp)
            cmPosition += v;
        else
            cmPosition -= v;
    }

    // 其中 上、下、Q、E代表平移
    // 左右代表旋转
    private void MainWindow_KeyDown(object sender,
                                    KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left: cmTheta -= cmDTheta;break;
            case Key.Right: cmTheta += cmDTheta;break;
            case Key.Up:MoveUD(true);break;
            case Key.Down: MoveUD(false);break;
            case Key.Q: MoveLR(true);break;
            case Key.E: MoveLR(false);break;
        }
        // 更新相机位置
        PositionCamera();
    }


    // 更新相机的位置
    protected virtual void PositionCamera()
    {
        cm.Position = cmPosition;
        cm.LookDirection = AngleToVector(cmTheta, 1);
        cm.UpDirection = new Vector3D(0, 0, 1);
    }

    public double CameraDR = 0.1;
    public double CameraDTheta = Math.PI / 30;
    public double CameraDPhi = Math.PI / 15;

    private double CameraR = 3000;
    private double CameraTheta = Math.PI / 3.0;
    private double CameraPhi = Math.PI / 3.0;

    private double[] origin = new double[3];

    private Point ptLast;
    private void mainWindow_LeftDown(object sender, MouseButtonEventArgs e)
    {
        mainWindow.CaptureMouse();
        mainWindow.MouseMove += MainWindow_MouseMove;
        mainWindow.MouseUp += MainWindow_MouseUp;
        ptLast = e.GetPosition(mainWindow);
    }

    private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        mainWindow.ReleaseMouseCapture();
        mainWindow.MouseMove -= MainWindow_MouseMove;
        mainWindow.MouseUp -= MainWindow_MouseUp;
    }

    private void MainWindow_MouseMove(object sender, MouseEventArgs e)
    {
        const double xscale = 0.1;
        const double yscale = 0.1;

        Point newPoint = e.GetPosition(mainWindow);
        double dx = newPoint.X - ptLast.X;
        double dy = newPoint.Y - ptLast.Y;

        CameraTheta -= dx * CameraDTheta * xscale;
        CameraPhi -= dy * CameraDPhi * yscale;

        ptLast = newPoint;
        PositionCameraMouse();
    }
    private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        this.CameraR -= e.Delta;
        this.PositionCameraMouse();
    }

    private void PositionCameraMouse()
    {        
        var y = origin[1] + CameraR * Math.Cos(CameraPhi);
        var h = CameraR * Math.Sin(CameraPhi);
        var x = origin[0] + h * Math.Sin(CameraTheta);
        var z = origin[2] + h * Math.Cos(CameraTheta);

        cm.Position = new Point3D(x, y, z);
        cm.LookDirection = new Vector3D(-x, -y, -z);
        cm.UpDirection = new Vector3D(0, 0, 1);
    }
}
