# PhotoDemon VB6 to .NET 10 MAUI - Testing Strategy

**Document Version:** 1.0
**Date:** 2025-11-17
**Agent:** Agent 12 - Testing Strategy
**Status:** Final

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Testing Objectives](#testing-objectives)
3. [Unit Test Strategy](#unit-test-strategy)
4. [Integration Test Strategy](#integration-test-strategy)
5. [UI Test Approach](#ui-test-approach)
6. [Performance Benchmarking Plan](#performance-benchmarking-plan)
7. [Regression Test Requirements](#regression-test-requirements)
8. [Test Infrastructure](#test-infrastructure)
9. [Quality Gates](#quality-gates)
10. [Risk-Based Testing Priorities](#risk-based-testing-priorities)
11. [Appendix: Test Data Management](#appendix-test-data-management)

---

## Executive Summary

This document defines a comprehensive testing strategy for the PhotoDemon VB6 to .NET 10 MAUI migration, ensuring feature parity, quality, and performance across all aspects of the application.

### Project Scope

| Metric | Count | Testing Requirement |
|--------|-------|-------------------|
| **Total Effects** | 117+ | Algorithm validation, visual regression |
| **Custom Controls** | 56 controls | UI component testing, behavior validation |
| **File Formats** | 40+ import, 25+ export | Format compatibility, round-trip testing |
| **Custom Parsers** | 15 native parsers | Parser accuracy, edge case handling |
| **Win32 API Calls** | 1,493 declarations | Interop testing, pointer validation |
| **Target Platforms** | Windows, macOS, Linux | Cross-platform validation |

### Key Quality Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Unit Test Coverage** | 80%+ | Code coverage analysis |
| **Critical Path Coverage** | 95%+ | Priority-based coverage |
| **Performance Tolerance** | Within 2x VB6 (most operations) | Automated benchmarks |
| **Performance Critical** | Within 1.5x VB6 (Gaussian blur, rendering) | BenchmarkDotNet |
| **File Compatibility** | 100% VB6 PDI files | Test corpus validation |
| **Visual Regression** | 95%+ similarity | Pixel comparison |
| **Zero Critical Bugs** | At release | Severity-based gate |

### Testing Framework Stack

**Recommended Stack:**
- **Unit Testing:** xUnit + FluentAssertions
- **UI Testing:** Appium (MAUI) + custom test harness
- **Performance:** BenchmarkDotNet
- **Visual Regression:** ImageSharp comparison utilities
- **Mocking:** Moq / NSubstitute
- **Code Coverage:** Coverlet / dotCover

---

## Testing Objectives

### Primary Objectives

1. **Feature Parity:** Validate that 100% of VB6 functionality is preserved in .NET MAUI
2. **Quality Assurance:** Ensure zero critical bugs and minimal medium/low severity issues
3. **Performance Validation:** Meet or exceed VB6 performance targets
4. **Cross-Platform Compatibility:** Verify consistent behavior on Windows, macOS, and Linux
5. **Regression Prevention:** Detect and prevent feature regressions throughout development

### Secondary Objectives

1. **Maintainability:** Build a sustainable, automated test suite
2. **Documentation:** Provide clear test reports and coverage metrics
3. **Early Detection:** Identify issues during development, not after release
4. **Confidence:** Enable rapid iteration with comprehensive safety net

---

## Unit Test Strategy

### 1.1 Testing Framework Selection

#### Recommended: xUnit

**Rationale:**
- **Modern design:** Built for .NET Core/.NET 10+
- **Parallel execution:** Tests run concurrently for faster execution
- **Extensibility:** Easy to extend with custom test collections, fixtures
- **Industry standard:** Wide adoption, excellent tooling support
- **MAUI compatible:** Works well with MAUI projects

**Alternative Consideration:** NUnit (if team has existing expertise)

**Installation:**
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.70" />
```

### 1.2 Unit Test Structure and Organization

#### Project Organization

```
PhotoDemon.Tests/
├── Unit/
│   ├── Core/
│   │   ├── ImageTests.cs
│   │   ├── LayerTests.cs
│   │   ├── BitmapTests.cs
│   │   └── SelectionTests.cs
│   ├── Effects/
│   │   ├── Adjustments/
│   │   │   ├── BrightnessContrastTests.cs
│   │   │   ├── CurvesTests.cs
│   │   │   └── ...
│   │   ├── Blur/
│   │   │   ├── GaussianBlurTests.cs
│   │   │   ├── BoxBlurTests.cs
│   │   │   └── ...
│   │   └── ... (all effect categories)
│   ├── Formats/
│   │   ├── PSD/
│   │   │   ├── PSDHeaderTests.cs
│   │   │   ├── PSDLayerTests.cs
│   │   │   ├── PSDCompressionTests.cs
│   │   │   └── PSDColorManagementTests.cs
│   │   ├── PSP/
│   │   │   ├── PSPParserTests.cs
│   │   │   └── ...
│   │   ├── XCF/
│   │   │   ├── XCFParserTests.cs
│   │   │   └── ...
│   │   └── ... (all formats)
│   ├── Interop/
│   │   ├── TypeMappingTests.cs
│   │   ├── StructureSizeTests.cs
│   │   ├── PointerTests.cs
│   │   └── MarshalingTests.cs
│   ├── Rendering/
│   │   ├── Graphics2DTests.cs
│   │   ├── BlendModeTests.cs
│   │   └── CompositorTests.cs
│   └── Utilities/
│       ├── ColorSpaceTests.cs
│       ├── MathUtilsTests.cs
│       └── StringUtilsTests.cs
├── Integration/
│   └── ... (integration tests)
└── TestData/
    ├── images/
    ├── pdi/
    ├── macros/
    └── reference/
```

#### Naming Conventions

**Test Class Naming:**
```csharp
// Pattern: [ClassName]Tests
public class GaussianBlurTests { }
public class PSDParserTests { }
public class ImageTests { }
```

**Test Method Naming:**
```csharp
// Pattern: [MethodUnderTest]_[Scenario]_[ExpectedResult]
[Fact]
public void ApplyGaussianBlur_WithRadius10_ProducesExpectedOutput() { }

[Fact]
public void LoadPSD_WithAllColorModes_SuccessfullyLoads() { }

[Theory]
[InlineData(8)]
[InlineData(16)]
[InlineData(32)]
public void LoadPSD_WithDifferentBitDepths_PreservesColorAccuracy(int bitsPerChannel) { }
```

### 1.3 Mock/Stub Strategy for Dependencies

#### Dependency Injection Setup

**Service Registration:**
```csharp
// In test project setup
public class TestFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        var services = new ServiceCollection();

        // Register real services
        services.AddSingleton<IImageProcessor, ImageProcessor>();

        // Register mocks for external dependencies
        services.AddSingleton<IFileSystem>(new MockFileSystem());
        services.AddSingleton<ILogger>(Mock.Of<ILogger>());

        ServiceProvider = services.BuildServiceProvider();
    }
}
```

#### Mock Strategy

**Use Mocks For:**
1. **External I/O:** File system, network, database
2. **UI interactions:** Progress reporting, user prompts
3. **Plugin dependencies:** External libraries not available in test environment
4. **System time:** Deterministic testing
5. **Random number generators:** Reproducible tests

**Example:**
```csharp
public class EffectProcessorTests
{
    [Fact]
    public void ApplyEffect_ReportsProgress_CallsProgressHandler()
    {
        // Arrange
        var mockProgress = new Mock<IProgress<int>>();
        var effect = new GaussianBlur();
        var image = CreateTestImage();

        // Act
        effect.Apply(image, mockProgress.Object);

        // Assert
        mockProgress.Verify(p => p.Report(It.IsAny<int>()), Times.AtLeastOnce);
    }
}
```

**Use Real Implementations For:**
1. **Core data structures:** Image, Layer, Bitmap
2. **Algorithms:** Effects, color conversions
3. **Parsers:** Custom file format parsers
4. **Math utilities:** Pure functions

### 1.4 Code Coverage Goals

#### Overall Coverage Targets

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| **Core Data Model** | 90%+ | Critical |
| **Effects (All 117+)** | 85%+ | High |
| **File Format Parsers** | 80%+ | High |
| **Rendering Engine** | 85%+ | High |
| **UI Controls** | 70%+ | Medium |
| **Utilities** | 80%+ | Medium |
| **Interop Layer** | 95%+ | Critical |
| **Overall Project** | 80%+ | Target |

#### Coverage Measurement

**Tools:**
- **Coverlet:** Cross-platform code coverage framework
- **ReportGenerator:** HTML/XML reports
- **Integration:** Azure DevOps / GitHub Actions

**Configuration:**
```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./TestResults/</CoverletOutput>
  <Threshold>80</Threshold>
  <ThresholdType>line,branch</ThresholdType>
</PropertyGroup>
```

**CI/CD Integration:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:TestResults/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### 1.5 Critical Units to Test

#### 1.5.1 Algorithms (Effects)

**Priority: HIGHEST**

**Test Categories:**

**A. Color Adjustments (28 effects)**
```csharp
public class BrightnessContrastTests
{
    [Theory]
    [InlineData(0, 0)]     // No change baseline
    [InlineData(50, 0)]    // Brightness only
    [InlineData(0, 50)]    // Contrast only
    [InlineData(50, 50)]   // Combined
    [InlineData(-50, -50)] // Negative values
    public void Apply_WithVariousParameters_ProducesExpectedResults(int brightness, int contrast)
    {
        // Arrange
        var image = CreateTestImage();
        var effect = new BrightnessContrast(brightness, contrast);

        // Act
        var result = effect.Apply(image);

        // Assert
        result.Should().NotBeNull();
        AssertVisualSimilarity(result, GetReferenceImage($"brightness_{brightness}_contrast_{contrast}.png"), tolerance: 0.01);
    }

    [Fact]
    public void Apply_PreservesAlphaChannel()
    {
        // Verify alpha channel is not modified
    }

    [Fact]
    public void Apply_HandlesEdgeCases_BlackImage()
    {
        // Test with pure black image
    }

    [Fact]
    public void Apply_HandlesEdgeCases_WhiteImage()
    {
        // Test with pure white image
    }
}
```

**B. Blur Effects (8 effects) - PERFORMANCE CRITICAL**
```csharp
public class GaussianBlurTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Apply_WithVariousRadii_ProducesExpectedBlur(int radius)
    {
        var image = CreateTestImage();
        var effect = new GaussianBlur(radius);

        var result = effect.Apply(image);

        // Validate blur quality vs reference
        AssertVisualSimilarity(result, GetReferenceImage($"gaussian_blur_r{radius}.png"), tolerance: 0.02);
    }

    [Fact]
    public void Apply_IIRImplementation_MatchesVB6Output()
    {
        // Validate that Deriche/Alvarez-Mazorra IIR algorithm produces identical results to VB6
        var image = LoadTestImage("standard_test.png");
        var vb6Reference = LoadTestImage("vb6_gaussian_blur_r10.png");

        var effect = new GaussianBlur(10);
        var result = effect.Apply(image);

        AssertPixelPerfect(result, vb6Reference, tolerance: 1); // Allow 1 bit difference due to floating point
    }

    [Fact]
    public void Apply_LargeRadius_CompletesInReasonableTime()
    {
        // Performance test: large blur should complete within time threshold
        var image = CreateLargeTestImage(4000, 4000);
        var effect = new GaussianBlur(100);

        var stopwatch = Stopwatch.StartNew();
        effect.Apply(image);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds for 4K image
    }
}
```

**C. Distortion Effects (12 effects)**
```csharp
public class PerspectiveTransformTests
{
    [Fact]
    public void Apply_4PointTransform_PreservesImageContent()
    {
        var image = CreateTestImage();
        var corners = new[] {
            new Point(0, 0), new Point(100, 10),
            new Point(10, 100), new Point(90, 90)
        };

        var effect = new PerspectiveTransform(corners);
        var result = effect.Apply(image);

        // Verify transformation is correct
        AssertTransformationCorrect(result, corners);
    }
}
```

**D. Advanced Algorithms - HIGHEST COMPLEXITY**
```csharp
public class AnisotropicDiffusionTests
{
    [Fact]
    public void Apply_EdgePreservingSmoothing_PreservesEdges()
    {
        var image = LoadTestImage("sharp_edges.png");
        var effect = new AnisotropicDiffusion(iterations: 10, kappa: 50);

        var result = effect.Apply(image);

        // Verify edges are preserved while noise is reduced
        var edgeStrength = MeasureEdgeStrength(result);
        var noiseLevel = MeasureNoiseLevel(result);

        edgeStrength.Should().BeGreaterThan(0.8); // 80% edge preservation
        noiseLevel.Should().BeLessThan(0.2);      // 20% noise remaining
    }
}

public class MeanShiftTests
{
    [Fact]
    public void Apply_IterativeClustering_ProducesSegmentation()
    {
        var image = LoadTestImage("segmentation_test.png");
        var effect = new MeanShift(spatialRadius: 16, colorRadius: 16);

        var result = effect.Apply(image);

        // Verify segmentation quality
        var segmentCount = CountSegments(result);
        segmentCount.Should().BeInRange(5, 20); // Expected segment range
    }
}
```

#### 1.5.2 File I/O (Custom Parsers)

**Priority: CRITICAL**

**A. PSD Parser - Apple Test Suite Compliance**
```csharp
public class PSDParserTests
{
    [Theory]
    [InlineData("Bitmap", 1)]
    [InlineData("Grayscale", 8)]
    [InlineData("Indexed", 8)]
    [InlineData("RGB", 8)]
    [InlineData("RGB", 16)]
    [InlineData("RGB", 32)]
    [InlineData("CMYK", 8)]
    [InlineData("Lab", 8)]
    [InlineData("Multichannel", 8)]
    public void LoadPSD_AllColorModes_LoadsCorrectly(string colorMode, int bitsPerChannel)
    {
        var psdFile = $"test_{colorMode}_{bitsPerChannel}bpc.psd";
        var parser = new PSDParser();

        var image = parser.Load(GetTestFilePath(psdFile));

        image.Should().NotBeNull();
        image.ColorMode.Should().Be(colorMode);
        image.BitsPerChannel.Should().Be(bitsPerChannel);
    }

    [Fact]
    public void LoadPSD_WithLayers_PreservesLayerStructure()
    {
        var parser = new PSDParser();
        var image = parser.Load(GetTestFilePath("multilayer.psd"));

        image.Layers.Should().HaveCount(5);
        image.Layers[0].Name.Should().Be("Background");
        image.Layers[1].BlendMode.Should().Be(BlendMode.Multiply);
        image.Layers[2].Opacity.Should().Be(128);
    }

    [Fact]
    public void LoadPSD_AppleTestSuite_PassesAllTests()
    {
        var appleTestFiles = GetAppleTestSuiteFiles();
        var parser = new PSDParser();

        foreach (var testFile in appleTestFiles)
        {
            var image = parser.Load(testFile);

            // Validate against reference image
            var reference = LoadReferenceImage(testFile);
            AssertColorManagedMatch(image, reference, tolerance: 0.001); // Very strict tolerance
        }
    }

    [Fact]
    public void LoadPSD_RLECompression_DecompressesCorrectly()
    {
        var parser = new PSDParser();
        var image = parser.Load(GetTestFilePath("rle_compressed.psd"));

        // Verify pixel data is correct
        AssertPixelPerfect(image, GetReferenceImage("rle_reference.png"));
    }

    [Fact]
    public void LoadPSD_ICCProfile_AppliesColorManagement()
    {
        var parser = new PSDParser();
        var image = parser.Load(GetTestFilePath("icc_profile.psd"));

        image.ICCProfile.Should().NotBeNull();
        // Verify color-managed rendering matches reference
    }
}
```

**B. PSP Parser**
```csharp
public class PSPParserTests
{
    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void LoadPSP_AllVersions_LoadsCorrectly(int version)
    {
        var parser = new PSPParser();
        var image = parser.Load(GetTestFilePath($"test_v{version}.psp"));

        image.Should().NotBeNull();
        image.Version.Should().Be(version);
    }

    [Fact]
    public void LoadPSP_WithLayerGroups_HandlesGroupStructure()
    {
        var parser = new PSPParser();
        var image = parser.Load(GetTestFilePath("groups.psp"));

        // PSP uses dummy layers for groups
        var groupLayers = image.Layers.Where(l => l.IsGroup);
        groupLayers.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void LoadPSP_VectorLayers_PreservesVectorData()
    {
        var parser = new PSPParser();
        var image = parser.Load(GetTestFilePath("vectors.psp"));

        // Vector data should be preserved (not rasterized)
        var vectorLayers = image.Layers.Where(l => l.HasVectorData);
        vectorLayers.Should().HaveCountGreaterThan(0);
    }
}
```

**C. XCF Parser - All Precisions**
```csharp
public class XCFParserTests
{
    [Theory]
    [InlineData("u8", 8, false)]
    [InlineData("u16", 16, false)]
    [InlineData("u32", 32, false)]
    [InlineData("half", 16, true)]
    [InlineData("float", 32, true)]
    [InlineData("double", 64, true)]
    public void LoadXCF_AllPrecisions_LoadsCorrectly(string precision, int bits, bool isFloat)
    {
        var parser = new XCFParser();
        var image = parser.Load(GetTestFilePath($"test_{precision}.xcf"));

        image.Should().NotBeNull();
        image.BitsPerChannel.Should().Be(bits);
        image.IsFloatingPoint.Should().Be(isFloat);
    }

    [Fact]
    public void LoadXCF_TileBasedLoading_LoadsAllTiles()
    {
        var parser = new XCFParser();
        var image = parser.Load(GetTestFilePath("large_tiled.xcf"));

        // Verify all tiles are loaded correctly
        AssertNoMissingTiles(image);
    }

    [Fact]
    public void LoadXCF_GZipCompressed_DecompressesCorrectly()
    {
        var parser = new XCFParser();
        var image = parser.Load(GetTestFilePath("compressed.xcf.gz"));

        // Verify decompression worked
        image.Should().NotBeNull();
        AssertPixelPerfect(image, GetReferenceImage("compressed_reference.png"));
    }
}
```

**D. PDI Format - Backward Compatibility**
```csharp
public class PDIFormatTests
{
    [Fact]
    public void LoadPDI_VB6CreatedFiles_LoadsCorrectly()
    {
        var testFiles = GetVB6PDITestCorpus(); // 100+ test files
        var parser = new PDIParser();

        foreach (var testFile in testFiles)
        {
            var image = parser.Load(testFile);

            image.Should().NotBeNull($"Failed to load {testFile}");

            // Verify round-trip
            var tempFile = Path.GetTempFileName();
            parser.Save(image, tempFile);
            var reloaded = parser.Load(tempFile);

            AssertPixelPerfect(image, reloaded, $"Round-trip failed for {testFile}");
        }
    }

    [Theory]
    [InlineData("lz4")]
    [InlineData("zstd")]
    [InlineData("deflate")]
    public void LoadPDI_AllCompressionFormats_DecompressesCorrectly(string compression)
    {
        var parser = new PDIParser();
        var image = parser.Load(GetTestFilePath($"test_{compression}.pdi"));

        image.Should().NotBeNull();
        AssertPixelPerfect(image, GetReferenceImage("uncompressed_reference.pdi"));
    }
}
```

#### 1.5.3 Data Structures

**Priority: CRITICAL**

**A. Interop Layer - Pointer Migration**
```csharp
public class StructureSizeTests
{
    [Fact]
    public void VerifyStructureSizes_32Bit()
    {
        // Verify all structure sizes match VB6 32-bit sizes
        Marshal.SizeOf<BITMAPINFOHEADER>().Should().Be(40);
        Marshal.SizeOf<RGBQuad>().Should().Be(4);
        Marshal.SizeOf<LOGFONTW>().Should().Be(92); // 32-bit
    }

    [Fact]
    public void VerifyStructureSizes_64Bit()
    {
        // Verify structure sizes on 64-bit platform
        if (IntPtr.Size == 8)
        {
            // Some structures will be larger on 64-bit
            Marshal.SizeOf<SAFEARRAY2D>().Should().Be(32); // vs 24 on 32-bit
        }
    }

    [Fact]
    public void VerifyStructurePacking()
    {
        // Verify [StructLayout] attributes are correct
        var type = typeof(BITMAPINFOHEADER);
        var attribute = type.GetCustomAttribute<StructLayoutAttribute>();

        attribute.Should().NotBeNull();
        attribute.Pack.Should().Be(1); // Explicit packing
    }
}

public class PointerMigrationTests
{
    [Fact]
    public void IntPtrConversion_32To64Bit_MaintainsCorrectness()
    {
        // Test that pointer conversions work correctly
        var testPointer = new IntPtr(0x12345678);

        // Verify pointer operations
        var result = PerformPointerOperation(testPointer);

        result.Should().NotBe(IntPtr.Zero);
    }

    [Fact]
    public void SafeArrayAccess_ViaSpan_MatchesVB6Behavior()
    {
        // Verify Span<T> access matches VB6 VarPtr/VarPtrArray behavior
        var bitmap = CreateTestBitmap(100, 100);

        var vb6Pixels = GetVB6PixelData(bitmap);
        var netPixels = GetNetPixelDataViaSpan(bitmap);

        vb6Pixels.Should().Equal(netPixels);
    }
}
```

**B. Core Data Model**
```csharp
public class ImageTests
{
    [Fact]
    public void CreateImage_WithMultipleLayers_MaintainsLayerOrder()
    {
        var image = new Image(800, 600);
        image.AddLayer(new Layer("Background"));
        image.AddLayer(new Layer("Layer 1"));
        image.AddLayer(new Layer("Layer 2"));

        image.Layers.Should().HaveCount(3);
        image.Layers[0].Name.Should().Be("Background");
        image.Layers[2].Name.Should().Be("Layer 2");
    }

    [Fact]
    public void CompositeImage_WithBlendModes_ProducesExpectedResult()
    {
        var image = CreateMultiLayerTestImage();

        var composite = image.Composite();

        AssertVisualSimilarity(composite, GetReferenceImage("composite_reference.png"), tolerance: 0.01);
    }

    [Fact]
    public void Image_LargeSize_HandlesMemoryCorrectly()
    {
        // Test 100 megapixel image
        var image = new Image(10000, 10000);

        // Verify memory usage is reasonable
        var memoryUsed = GC.GetTotalMemory(true);
        memoryUsed.Should().BeLessThan(2_000_000_000); // <2GB
    }
}

public class LayerTests
{
    [Theory]
    [InlineData(BlendMode.Normal)]
    [InlineData(BlendMode.Multiply)]
    [InlineData(BlendMode.Screen)]
    [InlineData(BlendMode.Overlay)]
    public void ApplyBlendMode_AllModes_ProducesCorrectResult(BlendMode mode)
    {
        var baseLayer = CreateTestLayer();
        var topLayer = CreateTestLayer();
        topLayer.BlendMode = mode;

        var result = BlendLayers(baseLayer, topLayer);

        AssertVisualSimilarity(result, GetReferenceImage($"blend_{mode}.png"), tolerance: 0.01);
    }
}
```

#### 1.5.4 Rendering Engine

**Priority: HIGH**

```csharp
public class Graphics2DTests
{
    [Fact]
    public void DrawImage_SkiaSharp_MatchesGDIPlusOutput()
    {
        var image = LoadTestImage("test.png");

        var gdiOutput = RenderWithGDIPlus(image);
        var skiaOutput = RenderWithSkiaSharp(image);

        // Allow minor differences due to anti-aliasing
        AssertVisualSimilarity(gdiOutput, skiaOutput, tolerance: 0.02);
    }

    [Fact]
    public void BlendModes_AllModes_MatchVB6Output()
    {
        var modes = Enum.GetValues<BlendMode>();

        foreach (var mode in modes)
        {
            var vb6Reference = LoadTestImage($"vb6_blend_{mode}.png");
            var netOutput = RenderWithBlendMode(mode);

            AssertVisualSimilarity(netOutput, vb6Reference, tolerance: 0.01);
        }
    }
}

public class CompositorTests
{
    [Fact]
    public void CompositeMultipleLayers_WithAlpha_ProducesCorrectResult()
    {
        var compositor = new Compositor();
        var layers = CreateTestLayerStack();

        var result = compositor.Composite(layers);

        AssertVisualSimilarity(result, GetReferenceImage("composite_reference.png"), tolerance: 0.01);
    }
}
```

### 1.6 Test Examples

#### Example 1: Color Space Conversion Test
```csharp
public class ColorSpaceTests
{
    [Theory]
    [InlineData(255, 0, 0, 0, 100, 50)]      // Red
    [InlineData(0, 255, 0, 120, 100, 50)]    // Green
    [InlineData(0, 0, 255, 240, 100, 50)]    // Blue
    [InlineData(128, 128, 128, 0, 0, 50)]    // Gray
    public void RGBToHSL_KnownColors_ConvertsCorrectly(
        byte r, byte g, byte b,
        double expectedH, double expectedS, double expectedL)
    {
        var rgb = new RGB(r, g, b);

        var hsl = ColorSpace.RGBToHSL(rgb);

        hsl.H.Should().BeApproximately(expectedH, 1.0);
        hsl.S.Should().BeApproximately(expectedS, 1.0);
        hsl.L.Should().BeApproximately(expectedL, 1.0);
    }

    [Fact]
    public void RGBToHSL_RoundTrip_PreservesOriginalColor()
    {
        var original = new RGB(123, 45, 67);

        var hsl = ColorSpace.RGBToHSL(original);
        var roundTrip = ColorSpace.HSLToRGB(hsl);

        roundTrip.R.Should().Be(original.R);
        roundTrip.G.Should().Be(original.G);
        roundTrip.B.Should().Be(original.B);
    }
}
```

#### Example 2: Effect Parameter Serialization Test
```csharp
public class EffectSerializationTests
{
    [Fact]
    public void SerializeToXML_GaussianBlur_PreservesAllParameters()
    {
        var effect = new GaussianBlur
        {
            Radius = 10,
            Quality = BlurQuality.High,
            EdgeHandling = EdgeMode.Clamp
        };

        var xml = effect.SerializeToXML();
        var deserialized = EffectFactory.CreateFromXML(xml) as GaussianBlur;

        deserialized.Should().NotBeNull();
        deserialized.Radius.Should().Be(10);
        deserialized.Quality.Should().Be(BlurQuality.High);
        deserialized.EdgeHandling.Should().Be(EdgeMode.Clamp);
    }

    [Fact]
    public void SerializeToXML_VB6Compatibility_LoadsVB6MacroFiles()
    {
        var vb6MacroXML = LoadVB6MacroFile("test_macro.pdm");

        var macro = MacroParser.LoadFromXML(vb6MacroXML);

        macro.Should().NotBeNull();
        macro.Version.Should().Be("8.2014");
        macro.Processes.Should().HaveCountGreaterThan(0);
    }
}
```

---

## Integration Test Strategy

### 2.1 Integration Testing Approach for Subsystems

#### Testing Levels

**Level 1: Component Integration**
- Test interaction between related classes within a subsystem
- Example: Image + Layer + Bitmap interaction

**Level 2: Subsystem Integration**
- Test interaction between major subsystems
- Example: Effects → Rendering → UI

**Level 3: System Integration**
- Test end-to-end workflows
- Example: Load image → Apply effect → Save image

#### Test Organization

```
PhotoDemon.Tests/
├── Integration/
│   ├── Effects/
│   │   ├── EffectPipelineTests.cs
│   │   └── EffectPreviewTests.cs
│   ├── FileIO/
│   │   ├── LoadSaveRoundTripTests.cs
│   │   └── FormatConversionTests.cs
│   ├── Rendering/
│   │   ├── CanvasRenderingTests.cs
│   │   └── LayerCompositingTests.cs
│   ├── Macro/
│   │   ├── MacroRecordingTests.cs
│   │   └── MacroPlaybackTests.cs
│   └── CrossCutting/
│       ├── UndoRedoTests.cs
│       └── MemoryManagementTests.cs
```

### 2.2 Testing Plugin Architecture

#### Plugin Loading Tests
```csharp
public class PluginSystemTests
{
    [Fact]
    public void LoadAllPlugins_AtStartup_LoadsSuccessfully()
    {
        var pluginManager = new PluginManager();

        pluginManager.LoadAll();

        pluginManager.LoadedPlugins.Should().HaveCountGreaterThan(10);
        pluginManager.LoadedPlugins.Should().Contain(p => p.Name == "libwebp");
        pluginManager.LoadedPlugins.Should().Contain(p => p.Name == "libheif");
    }

    [Fact]
    public void Plugin_MissingDependency_GracefulDegradation()
    {
        // Remove a plugin DLL
        var pluginManager = new PluginManager();

        pluginManager.LoadAll();

        // App should still function without the plugin
        pluginManager.Errors.Should().ContainSingle();
        pluginManager.IsPluginAvailable("libjxl").Should().BeFalse();
    }
}
```

#### Format Plugin Integration Tests
```csharp
public class FormatPluginTests
{
    [Fact]
    public void WebP_Plugin_LoadsAndSavesCorrectly()
    {
        var image = LoadTestImage("test.png");
        var webpPath = Path.GetTempFileName() + ".webp";

        // Save via plugin
        var exporter = new WebPExporter();
        exporter.Save(image, webpPath, quality: 90);

        // Load via plugin
        var importer = new WebPImporter();
        var loaded = importer.Load(webpPath);

        // Verify quality
        AssertVisualSimilarity(image, loaded, tolerance: 0.05); // Lossy compression tolerance
    }
}
```

### 2.3 Testing Service Layer Interactions

#### Service Integration Tests
```csharp
public class ServiceLayerTests
{
    [Fact]
    public async Task ImageService_LoadImageAsync_LoadsAndCachesCorrectly()
    {
        var imageService = GetService<IImageService>();
        var filePath = GetTestFilePath("test.png");

        var image = await imageService.LoadImageAsync(filePath);

        image.Should().NotBeNull();
        imageService.IsCached(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task EffectService_ApplyEffect_UpdatesUndoStack()
    {
        var effectService = GetService<IEffectService>();
        var undoService = GetService<IUndoService>();
        var image = CreateTestImage();

        await effectService.ApplyEffectAsync(image, new GaussianBlur(10));

        undoService.CanUndo.Should().BeTrue();
        undoService.UndoStackSize.Should().Be(1);
    }
}
```

### 2.4 Testing Cross-Platform File I/O

#### Platform-Specific Path Tests
```csharp
public class CrossPlatformFileIOTests
{
    [Fact]
    public void SaveImage_WithUnicodePath_SavesCorrectly()
    {
        var image = CreateTestImage();
        var path = Path.Combine(Path.GetTempPath(), "测试图片.png");

        var success = SaveImage(image, path);

        success.Should().BeTrue();
        File.Exists(path).Should().BeTrue();
    }

    [Theory]
    [InlineData("Windows", @"C:\Users\Test\image.png")]
    [InlineData("macOS", @"/Users/Test/image.png")]
    [InlineData("Linux", @"/home/test/image.png")]
    public void LoadImage_PlatformSpecificPath_LoadsCorrectly(string platform, string path)
    {
        if (GetCurrentPlatform() != platform)
            return; // Skip on other platforms

        // Create test file at platform-specific path
        var image = CreateTestImage();
        SaveImage(image, path);

        var loaded = LoadImage(path);

        loaded.Should().NotBeNull();
    }
}
```

### 2.5 Database/Settings Persistence Testing

#### Settings Migration Tests
```csharp
public class SettingsPersistenceTests
{
    [Fact]
    public void LoadSettings_VB6SettingsFile_MigratesCorrectly()
    {
        var vb6Settings = LoadVB6SettingsXML();
        var settingsManager = new SettingsManager();

        settingsManager.ImportVB6Settings(vb6Settings);

        settingsManager.GetSetting("Theme").Should().NotBeNull();
        settingsManager.GetSetting("Language").Should().NotBeNull();
    }

    [Fact]
    public void SaveSettings_RoundTrip_PreservesAllSettings()
    {
        var settingsManager = new SettingsManager();
        settingsManager.SetSetting("TestKey", "TestValue");

        settingsManager.Save();

        var newManager = new SettingsManager();
        newManager.Load();

        newManager.GetSetting("TestKey").Should().Be("TestValue");
    }
}
```

#### Undo/Redo Stack Integration Tests
```csharp
public class UndoRedoIntegrationTests
{
    [Fact]
    public void ApplyMultipleEffects_UndoAll_RestoresOriginalImage()
    {
        var image = LoadTestImage("test.png");
        var original = image.Clone();

        // Apply 3 effects
        ApplyEffect(image, new GaussianBlur(10));
        ApplyEffect(image, new BrightnessContrast(50, 0));
        ApplyEffect(image, new Sharpen(5));

        // Undo all 3
        Undo();
        Undo();
        Undo();

        // Should match original
        AssertPixelPerfect(image, original);
    }

    [Fact]
    public void UndoRedo_HeavyDiskUsage_HandlesLargeImages()
    {
        var largeImage = CreateLargeTestImage(10000, 10000); // 100MP

        // Apply effect that creates undo data
        ApplyEffect(largeImage, new GaussianBlur(10));

        // Verify undo data is on disk (not in memory)
        var undoService = GetService<IUndoService>();
        undoService.IsUsingDiskStorage.Should().BeTrue();

        // Undo should still work
        Undo();
        AssertPixelPerfect(largeImage, GetOriginalTestImage());
    }
}
```

---

## UI Test Approach

### 3.1 MAUI UI Testing Framework Options

#### Recommended: Appium + Custom Test Harness

**Appium for MAUI:**
- Cross-platform (Windows, macOS, iOS, Android)
- Industry standard
- WebDriver protocol
- Good MAUI support (as of .NET 8+)

**Alternative: Manual Testing Checklist + Limited Automation**
- UI automation for MAUI is still maturing
- Focus on critical path automation
- Comprehensive manual testing checklist for non-critical paths

**Installation:**
```xml
<PackageReference Include="Appium.WebDriver" Version="5.0.0" />
<PackageReference Include="Selenium.Support" Version="4.16.0" />
```

### 3.2 Automated UI Testing Strategy

#### Test Categories

**A. Control Behavior Tests**
```csharp
public class ControlBehaviorTests
{
    [Fact]
    public void pdSlider_ChangeValue_UpdatesDisplay()
    {
        var driver = GetAppiumDriver();

        var slider = driver.FindElement(By.Id("brightnessSlider"));
        var display = driver.FindElement(By.Id("brightnessValue"));

        slider.SendKeys(Keys.ArrowRight); // Increase value

        display.Text.Should().Be("51"); // Assuming started at 50
    }

    [Fact]
    public void pdCommandBar_ClickReset_RestoresDefaults()
    {
        var driver = GetAppiumDriver();

        // Change some values
        SetSliderValue("brightness", 75);
        SetSliderValue("contrast", 25);

        // Click Reset
        var resetButton = driver.FindElement(By.Id("resetButton"));
        resetButton.Click();

        // Verify defaults restored
        GetSliderValue("brightness").Should().Be(50);
        GetSliderValue("contrast").Should().Be(50);
    }
}
```

**B. Dialog Workflow Tests**
```csharp
public class EffectDialogTests
{
    [Fact]
    public void GaussianBlurDialog_ApplyEffect_UpdatesImage()
    {
        var driver = GetAppiumDriver();

        // Open effect dialog
        OpenMenu("Effects", "Blur", "Gaussian Blur");

        // Set radius
        SetSliderValue("radiusSlider", 20);

        // Verify preview updates
        var preview = driver.FindElement(By.Id("effectPreview"));
        preview.Should().NotBeNull();

        // Click OK
        ClickButton("okButton");

        // Verify effect was applied to main image
        // (This would require screenshot comparison or pixel inspection)
    }
}
```

**C. Canvas Interaction Tests**
```csharp
public class CanvasInteractionTests
{
    [Fact]
    public void Canvas_ZoomIn_UpdatesZoomLevel()
    {
        var driver = GetAppiumDriver();
        var canvas = driver.FindElement(By.Id("mainCanvas"));

        // Perform zoom gesture
        PerformPinchGesture(canvas, zoomFactor: 2.0);

        // Verify zoom level
        var zoomIndicator = driver.FindElement(By.Id("zoomLevel"));
        zoomIndicator.Text.Should().Contain("200%");
    }

    [Fact]
    public void Canvas_Pan_MovesViewport()
    {
        var driver = GetAppiumDriver();
        var canvas = driver.FindElement(By.Id("mainCanvas"));

        var initialViewport = GetViewportPosition();

        // Pan canvas
        PerformDragGesture(canvas, dx: 100, dy: 50);

        var newViewport = GetViewportPosition();

        newViewport.X.Should().Be(initialViewport.X + 100);
        newViewport.Y.Should().Be(initialViewport.Y + 50);
    }
}
```

### 3.3 Manual Testing Checklist

#### Critical Path Manual Testing

**Phase 1: Core Functionality (Release Blocker)**
- [ ] Application launches successfully on all platforms
- [ ] Can load image from file system
- [ ] Can save image in PDI format
- [ ] Can apply at least 1 effect from each category
- [ ] Undo/Redo works for basic operations
- [ ] UI is responsive (no freezing)

**Phase 2: Effects Testing (High Priority)**

For each of the 117+ effects:
- [ ] Effect dialog opens without error
- [ ] Preview displays correctly
- [ ] Parameters can be adjusted
- [ ] Preview updates in real-time
- [ ] OK button applies effect
- [ ] Cancel button discards changes
- [ ] Reset button restores defaults

**Systematic Approach:**
```
Create a spreadsheet with columns:
- Effect Name
- Category
- Dialog Opens (Y/N)
- Preview Works (Y/N)
- Parameters Adjustable (Y/N)
- Effect Applies Correctly (Y/N)
- Performance Acceptable (Y/N)
- Tester Initials
- Notes
```

**Phase 3: File Format Testing (High Priority)**

For each format:
- [ ] Can load file
- [ ] Can save file (if export supported)
- [ ] Round-trip preserves data
- [ ] Metadata preserved
- [ ] Animation preserved (if applicable)

**Phase 4: UI Control Testing (Medium Priority)**

For each of 56 custom controls:
- [ ] Control renders correctly
- [ ] Control responds to input
- [ ] Control updates display
- [ ] Control theme switches correctly
- [ ] Control works on touch devices

**Phase 5: Cross-Platform Validation (Medium Priority)**

Test on each platform:
- [ ] Windows 10
- [ ] Windows 11
- [ ] macOS 12+
- [ ] Linux (Ubuntu 22.04+)

For each platform:
- [ ] UI renders correctly
- [ ] Native file dialogs work
- [ ] Keyboard shortcuts work
- [ ] Performance is acceptable
- [ ] No platform-specific crashes

### 3.4 Cross-Platform UI Validation (Windows, macOS, Linux)

#### Platform-Specific UI Tests
```csharp
public class PlatformUITests
{
    [Theory]
    [InlineData("Windows")]
    [InlineData("macOS")]
    [InlineData("Linux")]
    public void MenuBar_AllPlatforms_DisplaysCorrectly(string platform)
    {
        if (GetCurrentPlatform() != platform)
            return;

        var driver = GetAppiumDriver();

        var menuBar = driver.FindElement(By.Id("mainMenuBar"));
        menuBar.Should().NotBeNull();

        // Platform-specific menu checks
        if (platform == "macOS")
        {
            // macOS has application menu
            var appMenu = driver.FindElement(By.Id("applicationMenu"));
            appMenu.Should().NotBeNull();
        }
    }

    [Fact]
    public void FileDialog_NativeIntegration_WorksCorrectly()
    {
        var driver = GetAppiumDriver();

        // Click "Open" button
        ClickButton("openButton");

        // Verify native file dialog appears
        // (Platform-specific verification)
        var fileDialog = WaitForNativeFileDialog();
        fileDialog.Should().NotBeNull();
    }
}
```

#### Rendering Consistency Tests
```csharp
public class RenderingConsistencyTests
{
    [Theory]
    [InlineData("Windows")]
    [InlineData("macOS")]
    [InlineData("Linux")]
    public void EffectRendering_AllPlatforms_ProducesSameOutput(string platform)
    {
        if (GetCurrentPlatform() != platform)
            return;

        var image = LoadTestImage("test.png");
        var effect = new GaussianBlur(10);

        var result = effect.Apply(image);

        // Compare with reference image (should be identical across platforms)
        var reference = LoadReferenceImage("gaussian_blur_r10_reference.png");
        AssertPixelPerfect(result, reference, tolerance: 1); // Allow 1-bit difference
    }
}
```

### 3.5 Accessibility Testing

#### Accessibility Requirements

**WCAG 2.1 Level AA Compliance:**
- Color contrast ratios: 4.5:1 for normal text, 3:1 for large text
- Keyboard navigation support
- Screen reader compatibility
- Focus indicators
- No reliance on color alone

#### Automated Accessibility Tests
```csharp
public class AccessibilityTests
{
    [Fact]
    public void Theme_DarkMode_MeetsContrastRequirements()
    {
        SetTheme(Theme.Dark);

        var foreground = GetThemeColor("foreground");
        var background = GetThemeColor("background");

        var contrastRatio = CalculateContrastRatio(foreground, background);

        contrastRatio.Should().BeGreaterThan(4.5); // WCAG AA standard
    }

    [Fact]
    public void AllControls_KeyboardNavigation_Accessible()
    {
        var driver = GetAppiumDriver();

        // Tab through all controls
        var focusableElements = driver.FindElements(By.XPath("//*[@focusable='true']"));

        foreach (var element in focusableElements)
        {
            element.SendKeys(Keys.Tab);

            // Verify focus indicator is visible
            var focusIndicator = element.GetCssValue("outline");
            focusIndicator.Should().NotBeNullOrEmpty();
        }
    }
}
```

#### Manual Accessibility Checklist

**Screen Reader Testing:**
- [ ] Test with Windows Narrator
- [ ] Test with macOS VoiceOver
- [ ] Test with NVDA (Windows)
- [ ] All UI elements have appropriate labels
- [ ] Reading order is logical

**Keyboard Navigation:**
- [ ] All functions accessible via keyboard
- [ ] Tab order is logical
- [ ] Shortcut keys documented
- [ ] No keyboard traps

**Visual Accessibility:**
- [ ] Color contrast meets WCAG AA
- [ ] UI is usable at 200% zoom
- [ ] No information conveyed by color alone
- [ ] Focus indicators are visible

---

## Performance Benchmarking Plan

### 4.1 Performance Baseline from VB6 Version

#### Baseline Methodology

**1. Identify Benchmark Operations**

**Critical Performance Metrics:**
| Operation | Importance | VB6 Baseline Target |
|-----------|-----------|-------------------|
| Gaussian Blur (1000x1000, r=10) | Critical | <200ms |
| Image Load (JPEG, 4K) | High | <500ms |
| Image Save (PNG, 4K) | High | <1000ms |
| Canvas Zoom/Pan (60fps) | Critical | 16ms per frame |
| Effect Preview Update | High | <100ms |
| Layer Composite (10 layers) | High | <300ms |

**2. Create VB6 Benchmark Suite**

Before migration, capture VB6 performance:
```
VB6 Benchmark Script:
1. Launch PhotoDemon VB6
2. Load test image (4000x3000 JPEG)
3. Apply Gaussian Blur (radius 10)
4. Measure time
5. Repeat 10 times
6. Calculate average, min, max, stddev
```

**3. Document VB6 Baseline**
```
Baseline Results (VB6 on Windows 10, i7-9700K, 16GB RAM):
- Gaussian Blur (r=10, 4K): 187ms avg, 165ms min, 210ms max
- JPEG Load (4K): 423ms avg
- PNG Save (4K): 892ms avg
- Canvas Render (60fps): 14ms avg per frame
```

### 4.2 Key Metrics to Track

#### Startup Time
- **Cold Start:** Time from launch to UI ready (target: <3s)
- **Warm Start:** Time with cached data (target: <1s)

#### Image Operations
- **Load Time:** Time to load various formats and sizes
- **Save Time:** Time to export to various formats
- **Thumbnail Generation:** Time to generate thumbnails

#### Effect Processing
- **Per-Effect Timing:** Time to process each of 117+ effects
- **Preview Generation:** Time to generate effect preview
- **Batch Processing:** Time per image in batch operation

#### UI Responsiveness
- **Canvas Frame Rate:** FPS during pan/zoom operations
- **Slider Update Latency:** Time from slider change to preview update
- **Dialog Launch Time:** Time to open effect dialogs

#### Memory Usage
- **Baseline Memory:** Memory usage with no image loaded
- **Per-Image Memory:** Memory increase per loaded image
- **Peak Memory:** Maximum memory during heavy operations
- **Memory Leaks:** Memory growth over time

### 4.3 Benchmarking Framework (BenchmarkDotNet)

#### Setup
```xml
<PackageReference Include="BenchmarkDotNet" Version="0.13.11" />
```

#### Benchmark Examples

**A. Effect Performance Benchmark**
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class EffectBenchmarks
{
    private Image _testImage;

    [GlobalSetup]
    public void Setup()
    {
        _testImage = LoadTestImage("benchmark_4k.png"); // 4000x3000
    }

    [Benchmark]
    public void GaussianBlur_Radius10()
    {
        var effect = new GaussianBlur(10);
        effect.Apply(_testImage);
    }

    [Benchmark]
    public void GaussianBlur_Radius50()
    {
        var effect = new GaussianBlur(50);
        effect.Apply(_testImage);
    }

    [Benchmark]
    public void BrightnessContrast()
    {
        var effect = new BrightnessContrast(50, 25);
        effect.Apply(_testImage);
    }

    [Benchmark]
    public void Sharpen()
    {
        var effect = new Sharpen(5);
        effect.Apply(_testImage);
    }
}

// Run benchmarks
BenchmarkRunner.Run<EffectBenchmarks>();
```

**B. File I/O Benchmark**
```csharp
[MemoryDiagnoser]
public class FileIOBenchmarks
{
    private string _testFilePath;
    private Image _testImage;

    [GlobalSetup]
    public void Setup()
    {
        _testFilePath = "benchmark_4k.jpg";
        _testImage = LoadTestImage(_testFilePath);
    }

    [Benchmark]
    public void LoadJPEG_4K()
    {
        var loader = new JPEGLoader();
        loader.Load(_testFilePath);
    }

    [Benchmark]
    public void LoadPNG_4K()
    {
        var loader = new PNGLoader();
        loader.Load("benchmark_4k.png");
    }

    [Benchmark]
    public void LoadPSD_Multilayer()
    {
        var loader = new PSDLoader();
        loader.Load("benchmark_multilayer.psd");
    }

    [Benchmark]
    public void SavePNG_4K_Quality100()
    {
        var exporter = new PNGExporter();
        exporter.Save(_testImage, Path.GetTempFileName(), compression: 9);
    }
}
```

**C. Rendering Benchmark**
```csharp
[MemoryDiagnoser]
public class RenderingBenchmarks
{
    private Image _image;
    private Layer[] _layers;

    [GlobalSetup]
    public void Setup()
    {
        _image = LoadTestImage("benchmark_4k.png");
        _layers = CreateLayerStack(10); // 10 layers
    }

    [Benchmark]
    public void CompositeLayerStack_10Layers()
    {
        var compositor = new Compositor();
        compositor.Composite(_layers);
    }

    [Benchmark]
    public void RenderCanvas_4K_SingleLayer()
    {
        var renderer = new CanvasRenderer();
        renderer.Render(_image, viewport: new Rectangle(0, 0, 1920, 1080), zoom: 1.0);
    }

    [Benchmark]
    public void RenderCanvas_4K_ZoomedIn()
    {
        var renderer = new CanvasRenderer();
        renderer.Render(_image, viewport: new Rectangle(0, 0, 1920, 1080), zoom: 4.0);
    }
}
```

#### Benchmark Result Analysis

**Output Format:**
```
| Method                          | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
|-------------------------------- |----------:|---------:|---------:|----------:|-------:|----------:|
| GaussianBlur_Radius10           | 195.2 ms  | 3.2 ms   | 2.9 ms   | 194.8 ms  | 1000   | 45.2 MB   |
| GaussianBlur_Radius50           | 287.4 ms  | 5.1 ms   | 4.7 ms   | 286.1 ms  | 1500   | 48.3 MB   |
| BrightnessContrast              | 42.3 ms   | 0.8 ms   | 0.7 ms   | 42.1 ms   | 200    | 12.1 MB   |
```

**Performance Target Analysis:**
```csharp
public class PerformanceValidator
{
    [Fact]
    public void ValidatePerformanceTargets()
    {
        var benchmarks = RunBenchmarks<EffectBenchmarks>();

        // VB6 baseline: Gaussian Blur r=10 averaged 187ms
        var gaussianBlur10 = benchmarks["GaussianBlur_Radius10"];

        // .NET target: within 2x of VB6 (374ms max)
        gaussianBlur10.Mean.Should().BeLessThan(374,
            "Gaussian Blur r=10 must be within 2x of VB6 baseline (187ms)");

        // Stretch goal: within 1.5x of VB6 (280ms)
        if (gaussianBlur10.Mean < 280)
        {
            Console.WriteLine("✓ Exceeds performance target (within 1.5x of VB6)");
        }
    }
}
```

### 4.4 Performance Regression Detection

#### Continuous Performance Monitoring

**CI/CD Integration:**
```yaml
# Azure DevOps Pipeline
- task: DotNetCoreCLI@2
  displayName: 'Run Performance Benchmarks'
  inputs:
    command: 'run'
    projects: '**/*Benchmarks.csproj'
    arguments: '--configuration Release'

- task: PublishBuildArtifacts@1
  displayName: 'Publish Benchmark Results'
  inputs:
    PathtoPublish: 'BenchmarkDotNet.Artifacts/results'
    ArtifactName: 'benchmark-results'
```

**Regression Detection:**
```csharp
public class PerformanceRegressionDetector
{
    [Fact]
    public void DetectPerformanceRegressions()
    {
        var currentResults = LoadCurrentBenchmarkResults();
        var baselineResults = LoadBaselineResults(); // From previous build

        foreach (var benchmark in currentResults)
        {
            var baseline = baselineResults[benchmark.Name];

            // Fail if performance regressed by >10%
            var regressionThreshold = baseline.Mean * 1.10;

            benchmark.Mean.Should().BeLessThan(regressionThreshold,
                $"{benchmark.Name} regressed by {((benchmark.Mean / baseline.Mean) - 1) * 100:F1}%");
        }
    }
}
```

**Performance Dashboard:**
- Track performance metrics over time
- Visualize trends (faster/slower)
- Alert on significant regressions
- Compare branches before merging

### 4.5 32-bit vs 64-bit Performance Comparison

#### Architecture-Specific Benchmarks
```csharp
[SimpleJob(RuntimeMoniker.Net80, Platform.X86)]
[SimpleJob(RuntimeMoniker.Net80, Platform.X64)]
public class ArchitectureBenchmarks
{
    private Image _testImage;

    [GlobalSetup]
    public void Setup()
    {
        _testImage = LoadTestImage("benchmark_4k.png");
    }

    [Benchmark]
    public void GaussianBlur_CrossArchitecture()
    {
        var effect = new GaussianBlur(10);
        effect.Apply(_testImage);
    }

    [Benchmark]
    public void PointerOperations_CrossArchitecture()
    {
        // Test pointer-heavy operations
        ProcessPixelsViaPointer(_testImage);
    }
}
```

**Expected Results:**
- **64-bit:** Generally faster due to more registers, better memory access
- **32-bit:** May be faster for specific operations due to smaller pointers
- **Memory:** 64-bit uses more memory (larger pointers)

**Analysis:**
```
| Method                  | Platform | Mean      | Allocated |
|------------------------ |--------- |----------:|----------:|
| GaussianBlur            | X86      | 210.3 ms  | 45.2 MB   |
| GaussianBlur            | X64      | 195.2 ms  | 48.3 MB   |
| PointerOperations       | X86      | 142.1 ms  | 32.1 MB   |
| PointerOperations       | X64      | 128.4 ms  | 38.2 MB   |
```

**Conclusion:**
- 64-bit is ~7% faster for Gaussian Blur
- 64-bit uses ~7% more memory
- Performance gain justifies memory overhead

---

## Regression Test Requirements

### 5.1 Feature Parity Validation Approach

#### Feature Checklist

**Complete Feature Matrix:**
```csv
Category,Feature,VB6 Status,MAUI Status,Test Coverage,Notes
Adjustments,Brightness/Contrast,✓,✓,100%,Passes all tests
Adjustments,Curves,✓,✓,95%,Edge cases pending
Effects,Gaussian Blur,✓,✓,100%,Performance validated
...
```

#### Automated Feature Parity Tests
```csharp
public class FeatureParityTests
{
    [Theory]
    [MemberData(nameof(GetAllEffects))]
    public void Effect_ExistsAndFunctional(string effectName, Type effectType)
    {
        // Verify effect class exists
        effectType.Should().NotBeNull($"{effectName} should be implemented");

        // Verify effect can be instantiated
        var effect = Activator.CreateInstance(effectType);
        effect.Should().NotBeNull();

        // Verify effect has required methods
        var applyMethod = effectType.GetMethod("Apply");
        applyMethod.Should().NotBeNull($"{effectName} must have Apply method");
    }

    public static IEnumerable<object[]> GetAllEffects()
    {
        // Return all 117+ effects
        yield return new object[] { "Gaussian Blur", typeof(GaussianBlur) };
        yield return new object[] { "Brightness Contrast", typeof(BrightnessContrast) };
        // ... all effects
    }
}
```

### 5.2 File Format Compatibility Testing

#### Backward Compatibility Tests

**A. VB6 PDI File Compatibility**
```csharp
public class PDIBackwardCompatibilityTests
{
    [Fact]
    public void LoadPDI_100VB6Files_AllLoadSuccessfully()
    {
        var testCorpus = GetVB6PDITestCorpus(); // 100+ real user files
        var failures = new List<string>();

        foreach (var pdiFile in testCorpus)
        {
            try
            {
                var image = LoadPDI(pdiFile);
                image.Should().NotBeNull();
            }
            catch (Exception ex)
            {
                failures.Add($"{pdiFile}: {ex.Message}");
            }
        }

        failures.Should().BeEmpty("All VB6 PDI files should load successfully");
    }

    [Fact]
    public void SavePDI_RoundTrip_PreservesAllData()
    {
        var original = LoadPDI("test_multilayer.pdi");

        var tempFile = Path.GetTempFileName();
        SavePDI(original, tempFile);

        var reloaded = LoadPDI(tempFile);

        // Verify all data preserved
        reloaded.Layers.Should().HaveCount(original.Layers.Count);
        AssertPixelPerfect(original, reloaded);
        AssertMetadataMatch(original, reloaded);
    }
}
```

**B. Cross-Format Compatibility**
```csharp
public class FormatCompatibilityTests
{
    [Theory]
    [InlineData("psd")]
    [InlineData("psp")]
    [InlineData("xcf")]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("gif")]
    [InlineData("webp")]
    public void LoadFormat_VariousFormats_LoadsCorrectly(string extension)
    {
        var testFile = GetTestFile(extension);

        var image = LoadImage(testFile);

        image.Should().NotBeNull();
        image.Width.Should().BeGreaterThan(0);
        image.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ConvertFormats_AllSupportedPairs_PreservesQuality()
    {
        var formats = new[] { "pdi", "psd", "png", "jpeg", "tiff" };

        foreach (var sourceFormat in formats)
        {
            foreach (var targetFormat in formats)
            {
                if (sourceFormat == targetFormat) continue;

                var source = LoadTestImage($"test.{sourceFormat}");
                var tempFile = Path.GetTempFileName() + $".{targetFormat}";

                SaveImage(source, tempFile);
                var converted = LoadImage(tempFile);

                // Allow some loss for lossy formats
                var tolerance = (targetFormat == "jpeg") ? 0.05 : 0.01;
                AssertVisualSimilarity(source, converted, tolerance);
            }
        }
    }
}
```

### 5.3 Visual Regression Testing for Effects

#### Visual Regression Testing Strategy

**A. Reference Image Generation**

**Process:**
1. Generate reference images using VB6 version
2. Apply each effect with known parameters
3. Save reference images for comparison

**B. Automated Visual Comparison**
```csharp
public class VisualRegressionTests
{
    [Theory]
    [MemberData(nameof(GetAllEffectTestCases))]
    public void Effect_VisualOutput_MatchesVB6Reference(
        string effectName,
        Effect effect,
        string referenceImagePath)
    {
        var testImage = LoadTestImage("standard_test.png");

        var result = effect.Apply(testImage);

        var reference = LoadReferenceImage(referenceImagePath);

        var similarity = CalculateImageSimilarity(result, reference);

        similarity.Should().BeGreaterThan(0.95,
            $"{effectName} should produce ≥95% similar output to VB6");
    }

    public static IEnumerable<object[]> GetAllEffectTestCases()
    {
        yield return new object[]
        {
            "Gaussian Blur r=10",
            new GaussianBlur(10),
            "references/gaussian_blur_r10.png"
        };

        yield return new object[]
        {
            "Brightness +50",
            new BrightnessContrast(brightness: 50, contrast: 0),
            "references/brightness_50.png"
        };

        // ... all 117+ effects with various parameters
    }
}
```

**C. Similarity Calculation**
```csharp
public class ImageSimilarityCalculator
{
    public double CalculateSimilarity(Image image1, Image image2)
    {
        if (image1.Width != image2.Width || image1.Height != image2.Height)
            return 0.0;

        long totalDifference = 0;
        long maxDifference = image1.Width * image1.Height * 255 * 3; // RGB

        for (int y = 0; y < image1.Height; y++)
        {
            for (int x = 0; x < image1.Width; x++)
            {
                var pixel1 = image1.GetPixel(x, y);
                var pixel2 = image2.GetPixel(x, y);

                totalDifference += Math.Abs(pixel1.R - pixel2.R);
                totalDifference += Math.Abs(pixel1.G - pixel2.G);
                totalDifference += Math.Abs(pixel1.B - pixel2.B);
            }
        }

        return 1.0 - ((double)totalDifference / maxDifference);
    }
}
```

**D. Perceptual Similarity (Advanced)**
```csharp
public class PerceptualSimilarity
{
    // Use SSIM (Structural Similarity Index) for better perceptual matching
    public double CalculateSSIM(Image image1, Image image2)
    {
        // SSIM implementation
        // Returns value between 0 (completely different) and 1 (identical)
        // Accounts for luminance, contrast, and structure
    }
}
```

### 5.4 Test Data Management

#### Test Data Organization

**Directory Structure:**
```
TestData/
├── images/
│   ├── standard/
│   │   ├── test_4k.png
│   │   ├── test_1080p.jpg
│   │   └── test_small.bmp
│   ├── formats/
│   │   ├── psd/
│   │   │   ├── rgb_8bit.psd
│   │   │   ├── cmyk_16bit.psd
│   │   │   └── lab_32bit.psd
│   │   ├── psp/
│   │   ├── xcf/
│   │   └── ...
│   ├── edge_cases/
│   │   ├── all_black.png
│   │   ├── all_white.png
│   │   ├── transparent.png
│   │   └── large_100mp.png
│   └── corrupted/
│       ├── truncated.jpg
│       ├── invalid_header.psd
│       └── corrupt_chunk.png
├── pdi/
│   ├── vb6_created/
│   │   ├── multilayer_v1.pdi
│   │   ├── compressed_lz4.pdi
│   │   └── ... (100+ files)
│   └── edge_cases/
├── macros/
│   ├── vb6_macros/
│   │   ├── simple_blur.pdm
│   │   ├── complex_workflow.pdm
│   │   └── ... (50+ files)
│   └── test_macros/
├── reference_outputs/
│   ├── effects/
│   │   ├── gaussian_blur_r10.png
│   │   ├── brightness_50.png
│   │   └── ... (all effect variations)
│   └── formats/
└── benchmarks/
    ├── benchmark_4k.png
    ├── benchmark_multilayer.psd
    └── benchmark_large.tiff
```

#### Test Data Management Strategy

**1. Version Control**
- Store small test files (<1MB) in Git
- Use Git LFS for larger files
- Store very large files externally (Azure Blob Storage, AWS S3)

**2. Test Data Acquisition**
```csharp
public class TestDataManager
{
    public static string GetTestFilePath(string filename)
    {
        var testDataRoot = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "TestData"
        );

        return Path.Combine(testDataRoot, filename);
    }

    public static async Task DownloadLargeTestFilesAsync()
    {
        // Download large test files from cloud storage if not present
        var largeFiles = new[]
        {
            "benchmark_100mp.png",
            "test_multilayer_50layers.psd"
        };

        foreach (var file in largeFiles)
        {
            var localPath = GetTestFilePath(file);
            if (!File.Exists(localPath))
            {
                await DownloadFromCloudAsync(file, localPath);
            }
        }
    }
}
```

**3. Test Corpus Collection**

**User-Contributed Files:**
- Request real-world PDI files from users
- Anonymize/sanitize if needed
- Cover various use cases (photo editing, graphics, etc.)

**Automated Generation:**
```csharp
public class TestImageGenerator
{
    public static Image GenerateGradient(int width, int height)
    {
        var image = new Image(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = new Color(
                    r: (byte)(x * 255 / width),
                    g: (byte)(y * 255 / height),
                    b: 128
                );
                image.SetPixel(x, y, color);
            }
        }

        return image;
    }

    public static Image GenerateNoise(int width, int height, int seed)
    {
        var random = new Random(seed);
        var image = new Image(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = new Color(
                    r: (byte)random.Next(256),
                    g: (byte)random.Next(256),
                    b: (byte)random.Next(256)
                );
                image.SetPixel(x, y, color);
            }
        }

        return image;
    }
}
```

### 5.5 Continuous Testing Strategy

#### CI/CD Pipeline Integration

**Build Pipeline:**
```yaml
# GitHub Actions Example
name: CI

on: [push, pull_request]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [windows-latest, macos-latest, ubuntu-latest]

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release

      - name: Run Unit Tests
        run: dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"

      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        with:
          name: test-results-${{ matrix.os }}
          path: TestResults/

      - name: Code Coverage Report
        uses: codecov/codecov-action@v3
        with:
          files: TestResults/*/coverage.cobertura.xml

  performance:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Run Performance Benchmarks
        run: dotnet run --project PhotoDemon.Benchmarks --configuration Release

      - name: Upload Benchmark Results
        uses: actions/upload-artifact@v3
        with:
          name: benchmark-results
          path: BenchmarkDotNet.Artifacts/

  visual-regression:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Download Test Data
        run: |
          # Download large test files
          pwsh -File Scripts/download-test-data.ps1

      - name: Run Visual Regression Tests
        run: dotnet test PhotoDemon.Tests.VisualRegression --configuration Release

      - name: Upload Visual Diff Images
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: visual-diffs
          path: TestResults/visual-diffs/
```

#### Test Execution Strategy

**Pre-Commit:**
- Fast unit tests only (<5 minutes)
- Critical path tests
- Code coverage check

**Pull Request:**
- All unit tests
- Integration tests
- Code coverage analysis
- Performance benchmarks (optional)

**Nightly Build:**
- All unit tests
- All integration tests
- Visual regression tests
- Performance benchmarks
- Cross-platform tests

**Release Candidate:**
- Complete test suite
- Manual testing checklist
- Performance validation
- Cross-platform validation
- Acceptance testing

---

## Test Infrastructure

### 6.1 Test Environment Setup

#### Development Environment
```
Required Software:
- .NET 10 SDK
- Visual Studio 2025 or JetBrains Rider
- Git + Git LFS
- Docker (for CI/CD simulation)

Optional:
- Windows Subsystem for Linux (WSL2) for Linux testing
- Parallels/VMware for macOS testing (if on Windows)
```

#### Test Execution Environments

**Local Development:**
- Unit tests run on developer machine
- Fast feedback loop
- Subset of integration tests

**CI/CD:**
- All tests run in cloud (GitHub Actions, Azure DevOps)
- Matrix testing across platforms
- Automated reporting

**Staging:**
- Full test suite
- Performance benchmarks
- Manual testing

### 6.2 Test Data Isolation

**Temporary File Management:**
```csharp
public class TestFixture : IDisposable
{
    private readonly string _tempDirectory;

    public TestFixture()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public string GetTempFilePath(string filename)
    {
        return Path.Combine(_tempDirectory, filename);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
```

### 6.3 Mocking External Dependencies

**File System Mock:**
```csharp
public class MockFileSystem : IFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new();

    public bool FileExists(string path) => _files.ContainsKey(path);

    public byte[] ReadAllBytes(string path) => _files[path];

    public void WriteAllBytes(string path, byte[] data) => _files[path] = data;
}
```

### 6.4 Parallel Test Execution

**xUnit Configuration:**
```csharp
// Assembly attribute for parallel execution
[assembly: CollectionBehavior(
    DisableTestParallelization = false,
    MaxParallelThreads = 4
)]

// Collection to force sequential execution when needed
[Collection("Sequential")]
public class SequentialTests
{
    // Tests that cannot run in parallel
}
```

---

## Quality Gates

### 7.1 Pre-Merge Quality Gates

**Required Checks:**
- [ ] All unit tests pass (100%)
- [ ] Code coverage ≥ 80%
- [ ] No critical bugs introduced
- [ ] Performance benchmarks pass (no regressions >10%)
- [ ] Code review approved
- [ ] Build succeeds on all platforms

### 7.2 Release Quality Gates

**Required for Release:**
- [ ] All automated tests pass
- [ ] Visual regression tests ≥ 95% match
- [ ] Performance targets met
- [ ] Manual testing checklist complete
- [ ] Zero critical bugs
- [ ] < 10 high-severity bugs
- [ ] Documentation updated
- [ ] Release notes prepared

### 7.3 Performance Quality Gates

**Targets:**
- Gaussian Blur (r=10, 4K): < 300ms (2x VB6 baseline)
- Canvas rendering: ≥ 30 FPS
- Image load (JPEG 4K): < 1000ms
- Memory usage: < 2GB for 100MP image

---

## Risk-Based Testing Priorities

### 8.1 Critical Risk Areas (from risk-assessment.md)

#### Priority 1: CRITICAL (Immediate Attention)

**T-001: 32-bit to 64-bit Pointer Migration**
- **Test Focus:** Structure size validation, pointer operations
- **Tests Required:**
  - All 176+ type declarations validated
  - Structure size tests (32-bit vs 64-bit)
  - SafeArray access via Span<T>
  - P/Invoke marshaling

**T-011: File Format Binary Compatibility (PDI)**
- **Test Focus:** PDI file backward compatibility
- **Tests Required:**
  - 100+ VB6-created PDI files load correctly
  - Round-trip tests preserve data
  - All compression formats work
  - Metadata preserved

**L-001: ImageSharp Commercial Licensing**
- **Mitigation:** Use Magick.NET + SkiaSharp instead
- **Tests Required:**
  - Verify all formats work with Magick.NET
  - Performance benchmarks
  - License compliance validation

#### Priority 2: HIGH (Active Management)

**P-001: Image Processing Performance Regression**
- **Test Focus:** Performance benchmarking
- **Tests Required:**
  - BenchmarkDotNet suite for all 117+ effects
  - Target: within 2x of VB6 for most, 1.5x for critical

**T-003: GDI+ to SkiaSharp Rendering**
- **Test Focus:** Rendering pipeline validation
- **Tests Required:**
  - Visual regression tests
  - Blend mode tests
  - Font rendering tests

**T-012: Custom Format Parser Porting (PSD, PSP, XCF)**
- **Test Focus:** Parser accuracy
- **Tests Required:**
  - Apple PSD test suite compliance
  - All PSP versions (v5-current)
  - All XCF precisions (8/16/32/64-bit int/float)

### 8.2 Test Prioritization Matrix

| Risk ID | Risk Name | Severity | Test Priority | Est. Test Effort |
|---------|-----------|----------|---------------|-----------------|
| T-001 | Pointer Migration | CRITICAL | P0 | 2 weeks |
| T-011 | PDI Compatibility | CRITICAL | P0 | 3 weeks |
| T-003 | GDI+ to SkiaSharp | CRITICAL | P0 | 4 weeks |
| T-012 | Custom Parsers | HIGH | P1 | 6 weeks |
| P-001 | Performance | HIGH | P1 | 4 weeks |
| T-004 | Custom Controls | HIGH | P1 | 8 weeks |
| ... | ... | ... | ... | ... |

---

## Appendix: Test Data Management

### A.1 Test File Locations

**Local Development:**
```
/PhotoDemon/Tests/TestData/
```

**CI/CD:**
```
Azure Blob Storage: photodemon-testdata
Bucket: testdata
URL: https://storage.azure.com/photodemon/testdata/
```

### A.2 Test Data Download Script

**PowerShell Script:**
```powershell
# download-test-data.ps1
param(
    [string]$StorageUrl = "https://storage.azure.com/photodemon/testdata/",
    [string]$LocalPath = "TestData/"
)

$files = @(
    "benchmark_4k.png",
    "benchmark_multilayer.psd",
    "pdi_corpus_100files.zip"
)

foreach ($file in $files) {
    $url = "$StorageUrl$file"
    $output = Join-Path $LocalPath $file

    if (!(Test-Path $output)) {
        Write-Host "Downloading $file..."
        Invoke-WebRequest -Uri $url -OutFile $output
    }
}

# Extract archives
Expand-Archive -Path (Join-Path $LocalPath "pdi_corpus_100files.zip") -DestinationPath $LocalPath
```

### A.3 Test Data Generation

**Generate Reference Images:**
```csharp
public class ReferenceImageGenerator
{
    [Fact]
    public void GenerateAllEffectReferences()
    {
        var testImage = LoadTestImage("standard_test.png");
        var outputDir = "reference_outputs/effects/";

        // Generate reference for each effect
        foreach (var effect in GetAllEffects())
        {
            var result = effect.Apply(testImage);
            var filename = $"{effect.Name.ToLower().Replace(" ", "_")}.png";

            SaveImage(result, Path.Combine(outputDir, filename));
        }
    }
}
```

---

## Summary

This testing strategy provides a comprehensive approach to ensuring the quality, performance, and feature parity of the PhotoDemon VB6 to .NET 10 MAUI migration.

### Key Takeaways

1. **Framework Selection:** xUnit + FluentAssertions + BenchmarkDotNet
2. **Coverage Target:** 80%+ overall, 95%+ critical paths
3. **Performance Target:** Within 2x VB6 for most operations, 1.5x for critical
4. **Visual Regression:** 95%+ similarity to VB6 reference outputs
5. **File Compatibility:** 100% VB6 PDI files must load correctly
6. **Cross-Platform:** Test on Windows, macOS, Linux

### Success Criteria

**The migration is successful if:**
- All 117+ effects are implemented and tested
- All 40+ import formats and 25+ export formats work correctly
- Custom parsers (PSD, PSP, XCF) maintain quality
- Performance targets are met
- Zero critical bugs at release
- 100% VB6 file compatibility
- Cross-platform functionality validated

---

**Document Prepared By:** Agent 12 - Testing Strategy Agent
**Date:** 2025-11-17
**Status:** Final
**Next Review:** After Phase 0 completion (Month 2)

---

**END OF DOCUMENT**
