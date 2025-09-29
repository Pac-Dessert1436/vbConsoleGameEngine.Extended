Option Strict On
Option Infer On
Imports System.Diagnostics.CodeAnalysis
Imports System.Security.Cryptography

Public Structure Vec2d
  Implements IFormattable

  Public x As Double
  Public y As Double

  Public Sub New(x As Double, y As Double)
    Me.x = x
    Me.y = y
  End Sub

  Public Function Norm() As Vec2d
    Dim mag = Me.Mag()
    Dim r = If(mag = 0, 0, 1 / mag)
    Return New Vec2d(x * r, y * r)
  End Function

  Public Function Perp() As Vec2d
    Return New Vec2d(-y, x)
  End Function

  Public Function Lerp(other As Vec2d, t As Double) As Vec2d
    Return New Vec2d(x + (other.x - x) * t, y + (other.y - y) * t)
  End Function

  Public Function Mag() As Double
    Return Math.Sqrt(x * x + y * y)
  End Function

  Public Function Mag2() As Double
    Return x * x + y * y
  End Function

  Public Function Dot(other As Vec2d) As Double
    Return x * other.x + y * other.y
  End Function

  Public Function Cross(other As Vec2d) As Double
    Return x * other.y - y * other.x
  End Function

  Public Function Rotate(rad As Double) As Vec2d
    Return New Vec2d(
      x * Math.Cos(rad) - y * Math.Sin(rad),
      x * Math.Sin(rad) + y * Math.Cos(rad)
    )
  End Function

  Public Shared Function Dist(vec1 As Vec2d, vec2 As Vec2d) As Double
    Return (vec1 - vec2).Mag()
  End Function

  Public Shared Function Dist2(vec1 As Vec2d, vec2 As Vec2d) As Double
    Return (vec1 - vec2).Mag2()
  End Function

  Public Shared Function TaxiDist(vec1 As Vec2d, vec2 As Vec2d) As Double
    Return Math.Abs(vec1.x - vec2.x) + Math.Abs(vec1.y - vec2.y)
  End Function

  Public Shared Function ChebDist(vec1 As Vec2d, vec2 As Vec2d) As Double
    Return Math.Max(Math.Abs(vec1.x - vec2.x), Math.Abs(vec1.y - vec2.y))
  End Function

  Public Shared Function Angle(vec1 As Vec2d, vec2 As Vec2d) As Double
    Return Math.Atan2(vec2.y - vec1.y, vec2.x - vec1.x)
  End Function

  Public Shared Operator -(vec As Vec2d) As Vec2d
    Return New Vec2d(-vec.x, -vec.y)
  End Operator

  Public Shared Operator +(left As Vec2d, right As Vec2d) As Vec2d
    Return New Vec2d(left.x + right.x, left.y + right.y)
  End Operator

  Public Shared Operator -(left As Vec2d, right As Vec2d) As Vec2d
    Return New Vec2d(left.x - right.x, left.y - right.y)
  End Operator

  Public Shared Operator *(left As Vec2d, right As Vec2d) As Vec2d
    Return New Vec2d(left.x * right.x, left.y * right.y)
  End Operator

  Public Shared Operator /(left As Vec2d, right As Vec2d) As Vec2d
    Return New Vec2d(left.x / right.x, left.y / right.y)
  End Operator

  Public Shared Operator *(left As Vec2d, num As Double) As Vec2d
    Return New Vec2d(left.x / num, left.y / num)
  End Operator

  Public Shared Operator /(left As Vec2d, num As Double) As Vec2d
    Return New Vec2d(left.x / num, left.y / num)
  End Operator

  Public Shared Operator =(left As Vec2d, right As Vec2d) As Boolean
    Return left.Equals(right)
  End Operator

  Public Shared Operator <>(left As Vec2d, right As Vec2d) As Boolean
    Return Not left.Equals(right)
  End Operator

  Public Overrides Function Equals(<NotNullWhen(True)> obj As Object) As Boolean
    Dim other As Vec2d = DirectCast(obj, Vec2d)
    Return x = other.x AndAlso y = other.y
  End Function

  Public Overrides Function GetHashCode() As Integer
    Return HashCode.Combine(x, y)
  End Function

  Public Overloads Function ToString(format As String, formatProvider As IFormatProvider) _
      As String Implements IFormattable.ToString
    If String.IsNullOrEmpty(format) Then Return $"({x},{y})"
    With format.ToUpperInvariant()
      If .StartsWith("F"c) Then
        Dim digits = CInt(.Remove(0, 1))
        Return String.Format("({0},{1})", Math.Round(x, digits), Math.Round(y, digits))
      Else
        Throw New FormatException($"Invalid format string: {format}")
      End If
    End With
  End Function
End Structure

Public Module GameMath
  Private m_rnd As New Random
  Private m_seed As Integer

  Public Property RandomSeed As Integer
    Get
      Return m_seed
    End Get
    Set(value As Integer)
      m_seed = value
      m_rnd = New Random(value)
    End Set
  End Property

  Public Function RandomRange(min As Integer, max As Integer) As Integer
    Return m_rnd.Next(min, max + 1)
  End Function

  Public Function RandomRange(min As Double, max As Double) As Double
    Return m_rnd.NextDouble() * (max - min) + min
  End Function

  Public Function MinkoDist(vec1 As Vec2d, vec2 As Vec2d, p As Integer) As Double
    If p <= 0 Then Throw New ArgumentException("Argument 'p' must be greater than 0.")
    If p = Integer.MaxValue Then Return Vec2d.ChebDist(vec1, vec2)
    With vec1 - vec2 : Return (Math.Abs(.x ^ p) + Math.Abs(.y ^ p)) ^ (1 / p) : End With
  End Function

  Public Function LevenDist(str1 As String, str2 As String) As Integer
    Dim len1 As Integer = str1.Length
    Dim len2 As Integer = str2.Length
    If len1 = 0 Then Return len2
    If len2 = 0 Then Return len1

    Dim matrix(len1, len2) As Integer
    For i As Integer = 0 To len1 : matrix(i, 0) = i : Next i
    For j As Integer = 0 To len2 : matrix(0, j) = j : Next j

    For i As Integer = 1 To len1
      For j As Integer = 1 To len2
        Dim cost = If(str1(i - 1) = str2(j - 1), 0, 1)
        matrix(i, j) = {
          matrix(i - 1, j) + 1, matrix(i, j - 1) + 1, matrix(i - 1, j - 1) + cost
        }.Min()
      Next j
    Next i
    Return matrix(len1, len2)
  End Function

  Public Function MahalDist(vec1 As Vec2d, vec2 As Vec2d, Optional covMat As Double(,) = Nothing) As Double
    If covMat Is Nothing Then covMat = New Double(,) {{1, 0}, {0, 1}}
    If covMat.GetLength(0) <> 2 OrElse covMat.GetLength(1) <> 2 Then
      Throw New ArgumentException("Covariance matrix must be 2x2.", NameOf(covMat))
    End If
    Dim InverseMatrix = Function(mat As Double(,))
                          Dim det = mat(0, 0) * mat(1, 1) - mat(0, 1) * mat(1, 0)
                          If det = 0 Then Throw New InvalidOperationException(
                            "Covariance matrix is singular and cannot be inverted.")
                          Return New Double(,) {
                            {mat(1, 1) / det, -mat(0, 1) / det},
                            {-mat(1, 0) / det, mat(0, 0) / det}
                          }
                        End Function
    Dim invCov = InverseMatrix(covMat)
    Dim diffT = vec1 - vec2
    ' Note: "diffT" is already the transpose of (vec1 - vec2), since you can think
    '       of it as though it is written vertically.
    Return Math.Sqrt(
      diffT.x * (invCov(0, 0) * diffT.x + invCov(1, 1) * diffT.y) +
      diffT.y * (invCov(0, 1) * diffT.x + invCov(1, 0) * diffT.y)
    )
  End Function

  Public Function CovMatrix(vec1 As Vec2d, vec2 As Vec2d, ParamArray others As Vec2d()) As Double(,)
    Dim vectors As New List(Of Vec2d) From {vec1, vec2}
    vectors.AddRange(others)
    If others.Length = 0 Then Throw New ArgumentException(
      "Must provide at least one more 2D vector outside of the first two.")
    Dim n As Integer = vectors.Count
    Dim meanX As Double = Aggregate vec As Vec2d In vectors Into Average(vec.x)
    Dim meanY As Double = Aggregate vec As Vec2d In vectors Into Average(vec.y)

    Dim covXX As Double = 0
    Dim covXY As Double = 0
    Dim covYY As Double = 0
    For Each vec As Vec2d In vectors
      Dim diffX = vec.x - meanX
      Dim diffY = vec.y - meanY
      covXX += diffX * diffX
      covXY += diffX * diffY
      covYY += diffY * diffY
    Next vec

    covXX /= n - 1
    covXY /= n - 1
    covYY /= n - 1
    Return New Double(,) {{covXX, covXY}, {covXY, covYY}}
  End Function

  Public Function Geohash(latitude As Double, longitude As Double, dateTime As Date) As Vec2d
    Dim opening As Byte() = Text.Encoding.ASCII.GetBytes(dateTime.ToString())
    Dim hex As String = Convert.ToHexString(MD5.HashData(opening))

    Const ALLOW_HEX_SPEC = Globalization.NumberStyles.AllowHexSpecifier
    Dim p As Double = Val("0." & ULong.Parse(hex.AsSpan(0, 16), ALLOW_HEX_SPEC))
    Dim q As Double = Val("0." & ULong.Parse(hex.AsSpan(16, 16), ALLOW_HEX_SPEC))
    Return New Vec2d(latitude + p, longitude + q)
  End Function
End Module