; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 132, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [132 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15424; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 30840; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 46272; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 61688; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 77104; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 92536; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 107952; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 123368; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 138800; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 154216; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 169632; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 185048; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 200464; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 215896; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 231312; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 246728; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 262160; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 277576; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 292992; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 308408; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 323824; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 339240; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 354672; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 370088; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 385504; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 400936; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 416368; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 431784; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 447216; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15392, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 462632; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 478024; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 493440; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 508872; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 524304; uint32_t buffer_offset
	}, ; 34: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 529936; uint32_t buffer_offset
	}, ; 35: CommunityToolkit.Mvvm
	%struct.CompressedAssemblyDescriptor {
		i32 31232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 548880; uint32_t buffer_offset
	}, ; 36: HarfBuzzSharp
	%struct.CompressedAssemblyDescriptor {
		i32 712704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 580112; uint32_t buffer_offset
	}, ; 37: LiveChartsCore
	%struct.CompressedAssemblyDescriptor {
		i32 21504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1292816; uint32_t buffer_offset
	}, ; 38: LiveChartsCore.Behaviours
	%struct.CompressedAssemblyDescriptor {
		i32 160256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1314320; uint32_t buffer_offset
	}, ; 39: LiveChartsCore.SkiaSharpView
	%struct.CompressedAssemblyDescriptor {
		i32 82432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1474576; uint32_t buffer_offset
	}, ; 40: LiveChartsCore.SkiaSharpView.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1557008; uint32_t buffer_offset
	}, ; 41: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1571344; uint32_t buffer_offset
	}, ; 42: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1578000; uint32_t buffer_offset
	}, ; 43: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1624592; uint32_t buffer_offset
	}, ; 44: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1651216; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1669136; uint32_t buffer_offset
	}, ; 46: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 16896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1688080; uint32_t buffer_offset
	}, ; 47: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1704976; uint32_t buffer_offset
	}, ; 48: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1882168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1714192; uint32_t buffer_offset
	}, ; 49: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 127544, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3596360; uint32_t buffer_offset
	}, ; 50: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 751104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3723904; uint32_t buffer_offset
	}, ; 51: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 53760, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4475008; uint32_t buffer_offset
	}, ; 52: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 207416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4528768; uint32_t buffer_offset
	}, ; 53: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 89088, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4736184; uint32_t buffer_offset
	}, ; 54: SkiaSharp
	%struct.CompressedAssemblyDescriptor {
		i32 22048, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4825272; uint32_t buffer_offset
	}, ; 55: SkiaSharp.HarfBuzz
	%struct.CompressedAssemblyDescriptor {
		i32 50232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4847320; uint32_t buffer_offset
	}, ; 56: SkiaSharp.Views.Android
	%struct.CompressedAssemblyDescriptor {
		i32 26168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4897552; uint32_t buffer_offset
	}, ; 57: SkiaSharp.Views.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 33864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4923720; uint32_t buffer_offset
	}, ; 58: SkiaSharp.Views.Maui.Core
	%struct.CompressedAssemblyDescriptor {
		i32 59392, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4957584; uint32_t buffer_offset
	}, ; 59: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 534016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5016976; uint32_t buffer_offset
	}, ; 60: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 16384, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5550992; uint32_t buffer_offset
	}, ; 61: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 16384, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5567376; uint32_t buffer_offset
	}, ; 62: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5583760; uint32_t buffer_offset
	}, ; 63: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 72192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5603216; uint32_t buffer_offset
	}, ; 64: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 540160, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5675408; uint32_t buffer_offset
	}, ; 65: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6215568; uint32_t buffer_offset
	}, ; 66: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6240144; uint32_t buffer_offset
	}, ; 67: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 43008, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6249872; uint32_t buffer_offset
	}, ; 68: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 209920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6292880; uint32_t buffer_offset
	}, ; 69: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 21504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6502800; uint32_t buffer_offset
	}, ; 70: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6524304; uint32_t buffer_offset
	}, ; 71: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6541712; uint32_t buffer_offset
	}, ; 72: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 12800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6573968; uint32_t buffer_offset
	}, ; 73: Xamarin.AndroidX.Lifecycle.ViewModelSavedState
	%struct.CompressedAssemblyDescriptor {
		i32 36352, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6586768; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 89088, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6623120; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.Navigation.Common
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6712208; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 58368, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6730640; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.Navigation.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 28160, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6789008; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 407040, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6817168; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 11264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7224208; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.SavedState
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7235472; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.Security.SecurityCrypto
	%struct.CompressedAssemblyDescriptor {
		i32 37888, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7259024; uint32_t buffer_offset
	}, ; 82: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 55808, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7296912; uint32_t buffer_offset
	}, ; 83: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7352720; uint32_t buffer_offset
	}, ; 84: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 584192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7391632; uint32_t buffer_offset
	}, ; 85: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 311296, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7975824; uint32_t buffer_offset
	}, ; 86: Xamarin.Google.Crypto.Tink.Android
	%struct.CompressedAssemblyDescriptor {
		i32 85504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8287120; uint32_t buffer_offset
	}, ; 87: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8372624; uint32_t buffer_offset
	}, ; 88: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 88576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8390032; uint32_t buffer_offset
	}, ; 89: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8478608; uint32_t buffer_offset
	}, ; 90: MaintManager.Shared
	%struct.CompressedAssemblyDescriptor {
		i32 652800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8490384; uint32_t buffer_offset
	}, ; 91: MaintManager.MAUI
	%struct.CompressedAssemblyDescriptor {
		i32 24064, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9143184; uint32_t buffer_offset
	}, ; 92: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9167248; uint32_t buffer_offset
	}, ; 93: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9182096; uint32_t buffer_offset
	}, ; 94: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 34816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9191312; uint32_t buffer_offset
	}, ; 95: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9226128; uint32_t buffer_offset
	}, ; 96: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9236880; uint32_t buffer_offset
	}, ; 97: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9252752; uint32_t buffer_offset
	}, ; 98: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9257872; uint32_t buffer_offset
	}, ; 99: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9270160; uint32_t buffer_offset
	}, ; 100: System.Diagnostics.TraceSource
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9281936; uint32_t buffer_offset
	}, ; 101: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9342352; uint32_t buffer_offset
	}, ; 102: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9364368; uint32_t buffer_offset
	}, ; 103: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9396112; uint32_t buffer_offset
	}, ; 104: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 354816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9402256; uint32_t buffer_offset
	}, ; 105: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 61440, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9757072; uint32_t buffer_offset
	}, ; 106: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9818512; uint32_t buffer_offset
	}, ; 107: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 125440, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9832848; uint32_t buffer_offset
	}, ; 108: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9958288; uint32_t buffer_offset
	}, ; 109: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9997200; uint32_t buffer_offset
	}, ; 110: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10004368; uint32_t buffer_offset
	}, ; 111: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10009488; uint32_t buffer_offset
	}, ; 112: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 73216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10027408; uint32_t buffer_offset
	}, ; 113: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 381952, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10100624; uint32_t buffer_offset
	}, ; 114: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10482576; uint32_t buffer_offset
	}, ; 115: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10491792; uint32_t buffer_offset
	}, ; 116: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 75264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10496912; uint32_t buffer_offset
	}, ; 117: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 13312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10572176; uint32_t buffer_offset
	}, ; 118: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 123904, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10585488; uint32_t buffer_offset
	}, ; 119: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10709392; uint32_t buffer_offset
	}, ; 120: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 372224, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10739088; uint32_t buffer_offset
	}, ; 121: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 155648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11111312; uint32_t buffer_offset
	}, ; 122: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11266960; uint32_t buffer_offset
	}, ; 123: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11272080; uint32_t buffer_offset
	}, ; 124: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11284368; uint32_t buffer_offset
	}, ; 125: System.Web.HttpUtility
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11295120; uint32_t buffer_offset
	}, ; 126: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11300240; uint32_t buffer_offset
	}, ; 127: System
	%struct.CompressedAssemblyDescriptor {
		i32 1900544, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11305360; uint32_t buffer_offset
	}, ; 128: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 170496, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13205904; uint32_t buffer_offset
	}, ; 129: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22560, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13376400; uint32_t buffer_offset
	}, ; 130: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2196992, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13398960; uint32_t buffer_offset
	} ; 131: Mono.Android
], align 4

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 15595952, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [15595952 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ e1d3646df9cb50b2a0924f5b67fa78f9750ae489"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
