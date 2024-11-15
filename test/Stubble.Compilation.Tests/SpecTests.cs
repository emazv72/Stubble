﻿using Stubble.Compilation.Settings;
using Stubble.Test.Shared.Spec;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Stubble.Compilation.Tests
{
    public class SpecTests
    {
        internal readonly ITestOutputHelper OutputStream;

        public SpecTests(ITestOutputHelper output)
        {
            OutputStream = output;
        }

        /*
        [Theory]
        [MemberData(nameof(Specs.SpecTests), MemberType = typeof(Specs))]
        public void CompilationRendererSpecTest(SpecTest data)
        {
            OutputStream.WriteLine(data.Name);
            var settings = CompilationSettings.GetDefaultRenderSettings();
            settings.CultureInfo = data.CultureInfo ?? settings.CultureInfo;

            var stubble = new StubbleCompilationRenderer();
            var output = data.Partials != null ? stubble.Compile(data.Template, data.Data, data.Partials, settings) : stubble.Compile(data.Template, data.Data, settings);

            var outputResult = output(data.Data);

            OutputStream.WriteLine("Expected \"{0}\", Actual \"{1}\"", data.Expected, outputResult);
            Assert.Equal(data.Expected, outputResult);
        }
       
        [Theory]
        [MemberData(nameof(Specs.SpecTests), MemberType = typeof(Specs))]
        public async Task CompilationRendererSpecTest_Async(SpecTest data)
        {
            OutputStream.WriteLine(data.Name);
            var settings = CompilationSettings.GetDefaultRenderSettings();
            settings.CultureInfo = data.CultureInfo ?? settings.CultureInfo;

            var stubble = new StubbleCompilationRenderer();
            var output = await (data.Partials != null ? stubble.CompileAsync(data.Template, data.Data, data.Partials, settings) : stubble.CompileAsync(data.Template, data.Data, settings));

            var outputResult = output(data.Data);

            OutputStream.WriteLine("Expected \"{0}\", Actual \"{1}\"", data.Expected, outputResult);
            Assert.Equal(data.Expected, outputResult);
        }
         */
    }
}
