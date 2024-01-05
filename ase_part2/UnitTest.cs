using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

[TestFixture]
public class CommandParserTests
{
    private CommandParser commandParser;
    private TextBox codeTextBox;
    private PictureBox displayArea;

    [SetUp]
    public void Setup()
    {
        codeTextBox = new TextBox();
        displayArea = new PictureBox();
        commandParser = new CommandParser(codeTextBox, displayArea);
    }

    [Test]
    public void TestMoveTo()
    {
        string command = "moveto 100 100";
        codeTextBox.Text = command;

        commandParser.ExecuteProgram(command);

        PointF expectedPosition = new PointF(100, 100);
        PointF actualPosition = commandParser.CurrentPosition;

        Assert.AreEqual(expectedPosition, actualPosition);
    }

    [Test]
    public void TestClearCommand()
    {
        // Set up a non-empty bitmap
        Bitmap bmp = new Bitmap(displayArea.Width, displayArea.Height);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
        }
        displayArea.Image = bmp;

        // Execute the Clear command
        string command = "clear";
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        // Check every pixel to ensure it's cleared 
        bool isCleared = true;
        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                if (bmp.GetPixel(x, y) != Color.White)
                {
                    isCleared = false;
                    break;
                }
            }
            if (!isCleared)
                break;
        }

        Assert.IsTrue(isCleared, "The canvas was not cleared correctly.");
    }

    [Test]
    public void TestRectangleCommand()
    {
        // Setup
        string command = "rectangle 50 30"; // Command to draw a rectangle of 50x30
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        Bitmap bmp = (Bitmap)displayArea.Image;
        Color expectedColor = Color.Black; // Assuming default pen color is black
        bool isRectangleDrawn = true;

        // Check the edges of the rectangle
        for (int x = 0; x < 50; x++)
        {
            if (bmp.GetPixel(x, 0) != expectedColor || bmp.GetPixel(x, 29) != expectedColor)
            {
                isRectangleDrawn = false;
                break;
            }
        }
        for (int y = 0; y < 30; y++)
        {
            if (bmp.GetPixel(0, y) != expectedColor || bmp.GetPixel(49, y) != expectedColor)
            {
                isRectangleDrawn = false;
                break;
            }
        }

        Assert.IsTrue(isRectangleDrawn, "Rectangle not drawn correctly.");
    }

    [Test]
    public void TestCircleCommand()
    {
        // Setup
        float radius = 20;
        string command = $"circle {radius}";
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        Bitmap bmp = (Bitmap)displayArea.Image;
        Color expectedColor = Color.Black; 
        bool isCircleDrawn = true;

        // Checking points on the circumference of the circle
        for (int angle = 0; angle < 360; angle++)
        {
            double radian = angle * Math.PI / 180;
            int x = (int)(radius * Math.Cos(radian)) + (int)radius; 
            int y = (int)(radius * Math.Sin(radian)) + (int)radius; 

            if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height || bmp.GetPixel(x, y) != expectedColor)
            {
                isCircleDrawn = false;
                break;
            }
        }

        Assert.IsTrue(isCircleDrawn, "Circle not drawn correctly.");
    }

    [Test]
    public void TestInvalidCommandException()
    {
        string command = "invalidCommand 10 20";
        codeTextBox.Text = command;

        Assert.Throws<ArgumentException>(() => commandParser.ExecuteProgram(command), "Invalid command did not throw an exception.");
    }

     [Test]
    public void TestPenPositionAfterDrawing()
    {
        string command = "moveto 10 10\ndrawto 20 20";
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);
        PointF expectedPosition = new PointF(20, 20);

        Assert.AreEqual(expectedPosition, commandParser.CurrentPosition, "Pen position not updated correctly after drawing.");
    }

    [Test]
public void TestRectangleFill()
{
    string command = "fill on\nrectangle 50 30";
    codeTextBox.Text = command;
    commandParser.ExecuteProgram(command);

    Bitmap bmp = (Bitmap)displayArea.Image;
    Color expectedColor = Color.Black; // Assuming default pen color is black and fill is on
    bool isFilled = true;

    // Check if the rectangle area is filled
    for (int x = 0; x < 50; x++)
    {
        for (int y = 0; y < 30; y++)
        {
            if (bmp.GetPixel(x, y) != expectedColor)
            {
                isFilled = false;
                break;
            }
        }
        if (!isFilled) break;
    }

    Assert.IsTrue(isFilled, "Rectangle fill did not work as expected.");
}

[Test]
public void TestCircleFill()
{
    float radius = 20;
    string command = $"fill on\ncircle {radius}";
    codeTextBox.Text = command;
    commandParser.ExecuteProgram(command);

    Bitmap bmp = (Bitmap)displayArea.Image;
    Color expectedColor = Color.Black;
    bool isFilled = true;

    // Check if the circle area is filled
    for (int x = 0; x < radius * 2; x++)
    {
        for (int y = 0; y < radius * 2; y++)
        {
            if (bmp.GetPixel(x, y) != expectedColor)
            {
                isFilled = false;
                break;
            }
        }
        if (!isFilled) break;
    }

    Assert.IsTrue(isFilled, "Circle fill did not work as expected.");
}


    [Test]
    public void TestPenPositionAfterDrawing()
    {
        string command = "moveto 10 10\ndrawto 20 20";
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);
        PointF expectedPosition = new PointF(20, 20);

        Assert.AreEqual(expectedPosition, commandParser.CurrentPosition, "Pen position not updated correctly after drawing.");
    }

    [Test]
    public void TestTriangleCommand()
    {
        string command = "triangle 10 10 50 10 30 40"; // Vertices of the triangle
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        Bitmap bmp = (Bitmap)displayArea.Image;
        Color expectedColor = Color.Black; 
        bool isTriangleDrawn = true;

        // Check if lines are drawn between the vertices
        isTriangleDrawn &= IsLineDrawn(bmp, new PointF(10, 10), new PointF(50, 10), expectedColor);
        isTriangleDrawn &= IsLineDrawn(bmp, new PointF(50, 10), new PointF(30, 40), expectedColor);
        isTriangleDrawn &= IsLineDrawn(bmp, new PointF(30, 40), new PointF(10, 10), expectedColor);

        Assert.IsTrue(isTriangleDrawn, "Triangle not drawn correctly.");
    }

    private bool IsLineDrawn(Bitmap bmp, PointF start, PointF end, Color expectedColor)
    {
        // Calculate the distance and angle between the points
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
        float angle = (float)Math.Atan2(dy, dx);

        // Iterate over the line
        for (float i = 0; i < distance; i += 1.0f)
        {
            int x = (int)(start.X + Math.Cos(angle) * i);
            int y = (int)(start.Y + Math.Sin(angle) * i);

            // Check if the pixel at (x, y) has the expected color
            if (bmp.GetPixel(x, y) != expectedColor)
            {
                return false;
            }
        }

        return true;
    }

    [Test]
    public void TestColorCommand()
    {
        string command = "color Red"; // Change color to red
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        Color expectedColor = Color.Red;
        Color actualColor = commandParser.CurrentPen.Color; 

        Assert.AreEqual(expectedColor, actualColor, "Pen color was not set correctly.");
    }

    [Test]
    public void TestFillToggleCommand()
    {
        string command = "fill on"; // Enable fill
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        bool expectedFillState = true; 
        bool actualFillState = commandParser.FillEnabled;

        Assert.AreEqual(expectedFillState, actualFillState, "Fill state was not toggled correctly.");

        // Test toggling it off as well
        command = "fill off";
        codeTextBox.Text = command;
        commandParser.ExecuteProgram(command);

        expectedFillState = false;
        actualFillState = commandParser.FillEnabled;

        Assert.AreEqual(expectedFillState, actualFillState, "Fill state was not toggled correctly.");
    }



    [TearDown]
    public void Cleanup()
    {
        commandParser.Cleanup();
    }
}
