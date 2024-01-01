using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

public class CommandParser
{
    private TextBox codeTextBox;
    private PictureBox displayArea;
    private Graphics graphics;
    private Pen currentPen;
    private PointF currentPosition;
    private bool fillEnabled = false;
    private float currentLineThickness = 1f;
    private float currentRotationAngle = 0f;
    private List<TextAnnotation> textAnnotations = new List<TextAnnotation>();
    private Bitmap currentDrawing;
    private Dictionary<string, float> variables = new Dictionary<string, float>();



    // Update the CurrentPen property to include line thickness
    public Pen CurrentPen => new Pen(currentPen.Color, currentLineThickness);

    public PointF CurrentPosition => currentPosition;
    public bool FillEnabled => fillEnabled;

    public CommandParser(TextBox codeTextBox, PictureBox displayArea)
    {
        this.codeTextBox = codeTextBox;
        this.displayArea = displayArea;
        this.currentPen = new Pen(Color.Black);
        this.currentLineThickness = 1f;
        this.currentRotationAngle = 0f;

        Bitmap bmp = new Bitmap(displayArea.Width, displayArea.Height);
        displayArea.Image = bmp;
        this.graphics = Graphics.FromImage(bmp);
        this.currentPen = new Pen(Color.Black);
        this.currentPosition = new PointF(0, 0);
        textAnnotations = new List<TextAnnotation>();
        currentDrawing = new Bitmap(displayArea.Width, displayArea.Height);
    }

    private class TextAnnotation
    {
        public string Text { get; set; }
        public PointF Position { get; set; }
        public Font Font { get; set; }
        public Color Color { get; set; }
    }
    public void ExecuteProgram(string program)
    {
        var lines = codeTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Trim().Split(' ');
            switch (parts[0].ToLower())
            {
                case "moveto":
                    MoveTo(float.Parse(parts[1]), float.Parse(parts[2]));
                    break;
                case "drawto":
                    DrawTo(float.Parse(parts[1]), float.Parse(parts[2]));
                    break;
                case "clear":
                    Clear();
                    break;
                case "rectangle":
                    DrawRectangle(float.Parse(parts[1]), float.Parse(parts[2]));
                    break;
                case "circle":
                    DrawCircle(float.Parse(parts[1]));
                    break;
                case "triangle":
                    DrawTriangle(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
                    break;
                case "color":
                    SetColor(Color.FromName(parts[1]));
                    break;
                case "reset":
                    ResetPenPosition();
                    break;
                case "fill":
                    ToggleFill(parts[1]);
                    break;
                case "linewidth":
                    SetLineThickness(ParseFloat(parts[1]));
                    break;
                case "rotate":
                    RotateShape(ParseFloat(parts[1]));
                    break;
                case "text":
                    AddTextAnnotation(string.Join(" ", parts.Skip(1)));
                    break;
                case "save":
                    SaveDrawing(string.Join(" ", parts.Skip(1)));
                    break;
                case "load":
                    LoadDrawing(string.Join(" ", parts.Skip(1)));
                    break;
                case "var":
                    DeclareVariable(parts[1], float.Parse(parts[2]));
                    break;
                case "set":
                    SetVariable(parts[1], float.Parse(parts[2]));
                    break;
                case "if":
                    if (!EvaluateCondition(parts.Skip(1).ToArray()))
                    {
                        SkipToNextEndIf(ref i, lines);
                    }
                    break;
                case "endif":
                    // Do nothing, it's just a marker for the end of an if block.
                    break;
                case "loop":
                    loopStack.Push(i);
                    inLoop = true;
                    break;
                case "endloop":
                    if (inLoop)
                    {
                        int loopStart = loopStack.Pop();
                        i = loopStart - 1; // Go back to the start of the loop
                    }
                    else
                    {
                        throw new InvalidOperationException("Found 'endloop' without a matching 'loop'.");
                    }
                    break;

                default:
                    throw new ArgumentException("Unknown command");
            }
        }

        displayArea.Invalidate();
    }



    public void ExecuteCommand(string command)
    {
        string[] lines = command.Split(' ');
        switch (lines[0].ToLower())
        {
            case "moveto":
                MoveTo(ParseFloat(lines[1]), ParseFloat(lines[2]));
                break;
            case "drawto":
                DrawTo(ParseFloat(lines[1]), ParseFloat(lines[2]));
                break;
            case "clear":
                Clear();
                break;
            case "rectangle":
                DrawRectangle(ParseFloat(lines[1]), ParseFloat(lines[2]));
                break;
            case "circle":
                DrawCircle(ParseFloat(lines[1]));
                break;
            case "triangle":
                DrawTriangle(ParseFloat(lines[1]), ParseFloat(lines[2]), ParseFloat(lines[3]), ParseFloat(lines[4]), ParseFloat(lines[5]), ParseFloat(lines[6]));
                break;
            case "color":
                SetColor(Color.FromName(lines[1]));
                break;
            case "reset":
                ResetPenPosition();
                break;
            case "fill":
                ToggleFill(lines[1]);
                break;
            default:
                throw new ArgumentException($"Unknown command: {lines[0]}");
        }
        displayArea.Invalidate();
    }

    private void SetColor(Color color, int opacity)
    {
        currentPen.Color = Color.FromArgb(opacity, color);
    }

    private void RotateShape(float angleDegrees)
    {
        currentRotationAngle = angleDegrees;
    }

    private void SetLineThickness(float thickness)  
    {
        currentLineThickness = thickness;
    }

    private void DeclareVariable(string variableName, float value)
{
    if (!variables.ContainsKey(variableName))
    {
        variables[variableName] = value;
    }
    else
    {
        throw new ArgumentException($"Variable '{variableName}' already exists.");
    }
}

private void SetVariable(string variableName, float value)
{
    if (variables.ContainsKey(variableName))
    {
        variables[variableName] = value;
    }
    else
    {
        throw new ArgumentException($"Variable '{variableName}' does not exist.");
    }
}

public float GetVariableValue(string variableName)
{
    if (variables.ContainsKey(variableName))
    {
        return variables[variableName];
    }
    else
    {
        throw new ArgumentException($"Variable '{variableName}' does not exist.");
    }
}
    private void AddTextAnnotation(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            // Define text annotation properties (e.g., font, color)
            Font font = new Font("Arial", 12); 
            Color textColor = Color.Black; 

            // Create a new text annotation and add it to the list
            var textAnnotation = new TextAnnotation
            {
                Text = text,
                Position = currentPosition,
                Font = font,
                Color = textColor
            };

            textAnnotations.Add(textAnnotation);
        }
    }

    private void SaveDrawing(string fileName)
    {
        if (currentDrawing != null)
        {
            try
            {
                currentDrawing.Save(fileName + ".png"); // Save as PNG image (you can choose a different format)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving the drawing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void LoadDrawing(string fileName)
    {
        try
        {
            var loadedImage = Image.FromFile(fileName);
            currentDrawing = new Bitmap(loadedImage);
            graphics = Graphics.FromImage(currentDrawing);
            displayArea.Image = currentDrawing;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading the image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private float ParseFloat(string input)
    {
        if (float.TryParse(input, out float result))
            return result;
        throw new ArgumentException($"Unable to parse '{input}' as a float.");
    }

    public void SaveProgram(string filePath)
    {
        File.WriteAllText(filePath, codeTextBox.Text);
    }

    public void LoadProgram(string filePath)
    {
        codeTextBox.Text = File.ReadAllText(filePath);
    }

    public void CheckSyntax()
    {
        var commands = codeTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var command in commands)
        {
            if (!IsValidCommand(command.Trim()))
            {
                MessageBox.Show($"Syntax error in command: {command}", "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        MessageBox.Show("All commands have valid syntax.", "Syntax Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private bool IsValidCommand(string command)
    {
        var lines = command.Split(' ');
        var commandType = lines[0].ToLower();

        try
        {
            switch (commandType)
            {
                case "moveto":
                case "drawto":
                    return lines.Length == 3 && lines.Skip(1).All(p => float.TryParse(p, out _));
                case "rectangle":
                    return lines.Length == 5 && lines.Skip(1).All(p => float.TryParse(p, out _));
                case "circle":
                    return lines.Length == 2 && float.TryParse(lines[1], out _);
                case "triangle":
                    return lines.Length == 7 && lines.Skip(1).All(p => float.TryParse(p, out _));
                case "color":
                    return lines.Length == 2 && Enum.IsDefined(typeof(KnownColor), lines[1]);
                case "clear":
                case "reset":
                    return lines.Length == 1;
                case "fill":
                    return lines.Length == 2 && (lines[1].ToLower() == "on" || lines[1].ToLower() == "off");
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void MoveTo(float x, float y)
    {
        currentPosition = new PointF(x, y);
    }

    private void DrawTo(float x, float y)
    {
        PointF newPosition = new PointF(x, y);
        graphics.DrawLine(currentPen, currentPosition, newPosition);
        currentPosition = newPosition;
    }

    private void Clear()
    {
        graphics.Clear(Color.White);
        currentPosition = new PointF(0, 0);
    }

    private void DrawRectangle(float width, float height)
    {
        // Apply rotation using currentRotationAngle before drawing
        graphics.TranslateTransform(currentPosition.X, currentPosition.Y);
        graphics.RotateTransform(currentRotationAngle);

        if (fillEnabled)
            graphics.FillRectangle(currentPen.Brush, -width / 2, -height / 2, width, height);
        else
            graphics.DrawRectangle(currentPen, -width / 2, -height / 2, width, height);

        // Reset rotation transform
        graphics.ResetTransform();
    }

    private void DrawCircle(float radius)
    {
        // Apply rotation using currentRotationAngle before drawing
        graphics.TranslateTransform(currentPosition.X, currentPosition.Y);
        graphics.RotateTransform(currentRotationAngle);

        if (fillEnabled)
            graphics.FillEllipse(currentPen.Brush, -radius, -radius, radius * 2, radius * 2);
        else
            graphics.DrawEllipse(currentPen, -radius, -radius, radius * 2, radius * 2);

        // Reset rotation transform
        graphics.ResetTransform();
    }

    private void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        PointF[] points = { new PointF(x1, y1), new PointF(x2, y2), new PointF(x3, y3) };

        // Apply rotation using currentRotationAngle before drawing
        graphics.TranslateTransform(currentPosition.X, currentPosition.Y);
        graphics.RotateTransform(currentRotationAngle);

        if (fillEnabled)
            graphics.FillPolygon(currentPen.Brush, points);
        else
            graphics.DrawPolygon(currentPen, points);

        // Reset rotation transform
        graphics.ResetTransform();
    }

    private void SetColor(Color color)
    {
        currentPen.Color = color;
    }

    private void ResetPenPosition()
    {
        currentPosition = new PointF(0, 0);
    }

    private void ToggleFill(string state)
    {
        fillEnabled = state.ToLower() == "on";
    }

    public void SetupGraphics(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
    }

    public void Cleanup()
    {
        if (graphics != null)
            graphics.Dispose();
        if (currentPen != null)
            currentPen.Dispose();
    }

}
