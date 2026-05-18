; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 136, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [136 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15664; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 31328; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 47000; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 62664; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 78288; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 93960; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 109624; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 125256; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 140920; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 156552; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 172224; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 187848; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 203520; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 219192; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 234816; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 250480; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 266152; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 281784; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 297408; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 313032; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 328704; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 344368; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 360032; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 375656; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 391288; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 406952; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 422576; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 438248; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 453912; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 469584; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 485256; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 500928; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 516600; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 532224; uint32_t buffer_offset
	}, ; 34: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 538368; uint32_t buffer_offset
	}, ; 35: CommunityToolkit.Mvvm
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 557312; uint32_t buffer_offset
	}, ; 36: HarfBuzzSharp
	%struct.CompressedAssemblyDescriptor {
		i32 772608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 585984; uint32_t buffer_offset
	}, ; 37: LiveChartsCore
	%struct.CompressedAssemblyDescriptor {
		i32 134656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1358592; uint32_t buffer_offset
	}, ; 38: LiveChartsCore.SkiaSharpView
	%struct.CompressedAssemblyDescriptor {
		i32 366080, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1493248; uint32_t buffer_offset
	}, ; 39: LiveChartsCore.SkiaSharpView.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1859328; uint32_t buffer_offset
	}, ; 40: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1874176; uint32_t buffer_offset
	}, ; 41: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1880832; uint32_t buffer_offset
	}, ; 42: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1927424; uint32_t buffer_offset
	}, ; 43: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1954048; uint32_t buffer_offset
	}, ; 44: Microsoft.Extensions.Diagnostics.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1962240; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.FileProviders.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1969408; uint32_t buffer_offset
	}, ; 46: Microsoft.Extensions.Hosting.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1975552; uint32_t buffer_offset
	}, ; 47: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1993472; uint32_t buffer_offset
	}, ; 48: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 16896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2012928; uint32_t buffer_offset
	}, ; 49: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2029824; uint32_t buffer_offset
	}, ; 50: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1925392, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2039040; uint32_t buffer_offset
	}, ; 51: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 133432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3964432; uint32_t buffer_offset
	}, ; 52: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 853504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4097864; uint32_t buffer_offset
	}, ; 53: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 61440, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4951368; uint32_t buffer_offset
	}, ; 54: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 208648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5012808; uint32_t buffer_offset
	}, ; 55: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 98304, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5221456; uint32_t buffer_offset
	}, ; 56: SkiaSharp
	%struct.CompressedAssemblyDescriptor {
		i32 24136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5319760; uint32_t buffer_offset
	}, ; 57: SkiaSharp.HarfBuzz
	%struct.CompressedAssemblyDescriptor {
		i32 50224, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5343896; uint32_t buffer_offset
	}, ; 58: SkiaSharp.Views.Android
	%struct.CompressedAssemblyDescriptor {
		i32 26152, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5394120; uint32_t buffer_offset
	}, ; 59: SkiaSharp.Views.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 33864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5420272; uint32_t buffer_offset
	}, ; 60: SkiaSharp.Views.Maui.Core
	%struct.CompressedAssemblyDescriptor {
		i32 73728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5454136; uint32_t buffer_offset
	}, ; 61: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 583680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5527864; uint32_t buffer_offset
	}, ; 62: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6111544; uint32_t buffer_offset
	}, ; 63: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6129464; uint32_t buffer_offset
	}, ; 64: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6148408; uint32_t buffer_offset
	}, ; 65: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 78336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6170936; uint32_t buffer_offset
	}, ; 66: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 593920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6249272; uint32_t buffer_offset
	}, ; 67: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6843192; uint32_t buffer_offset
	}, ; 68: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6869816; uint32_t buffer_offset
	}, ; 69: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 47104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6879544; uint32_t buffer_offset
	}, ; 70: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 236032, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6926648; uint32_t buffer_offset
	}, ; 71: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7162680; uint32_t buffer_offset
	}, ; 72: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7186232; uint32_t buffer_offset
	}, ; 73: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7205176; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7237944; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7251768; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 92672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7291192; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.Navigation.Common.Android
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7383864; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 65536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7403320; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.Navigation.Runtime.Android
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7468856; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 457728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7495992; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7953720; uint32_t buffer_offset
	}, ; 82: Xamarin.AndroidX.SavedState.SavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7966008; uint32_t buffer_offset
	}, ; 83: Xamarin.AndroidX.Security.SecurityCrypto
	%struct.CompressedAssemblyDescriptor {
		i32 41984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7990584; uint32_t buffer_offset
	}, ; 84: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 62976, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8032568; uint32_t buffer_offset
	}, ; 85: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8095544; uint32_t buffer_offset
	}, ; 86: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 675840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8135992; uint32_t buffer_offset
	}, ; 87: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 345088, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8811832; uint32_t buffer_offset
	}, ; 88: Xamarin.Google.Crypto.Tink.Android
	%struct.CompressedAssemblyDescriptor {
		i32 90624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9156920; uint32_t buffer_offset
	}, ; 89: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9247544; uint32_t buffer_offset
	}, ; 90: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 91648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9276216; uint32_t buffer_offset
	}, ; 91: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9367864; uint32_t buffer_offset
	}, ; 92: MaintManager.Shared
	%struct.CompressedAssemblyDescriptor {
		i32 717312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9379640; uint32_t buffer_offset
	}, ; 93: MaintManager.MAUI
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10096952; uint32_t buffer_offset
	}, ; 94: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10121528; uint32_t buffer_offset
	}, ; 95: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10137400; uint32_t buffer_offset
	}, ; 96: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 35328, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10148152; uint32_t buffer_offset
	}, ; 97: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10183480; uint32_t buffer_offset
	}, ; 98: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 94720, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10195256; uint32_t buffer_offset
	}, ; 99: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10289976; uint32_t buffer_offset
	}, ; 100: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10295096; uint32_t buffer_offset
	}, ; 101: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10306872; uint32_t buffer_offset
	}, ; 102: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10347320; uint32_t buffer_offset
	}, ; 103: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10359096; uint32_t buffer_offset
	}, ; 104: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10364216; uint32_t buffer_offset
	}, ; 105: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10424632; uint32_t buffer_offset
	}, ; 106: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10446648; uint32_t buffer_offset
	}, ; 107: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10478392; uint32_t buffer_offset
	}, ; 108: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 354816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10485048; uint32_t buffer_offset
	}, ; 109: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 61440, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10839864; uint32_t buffer_offset
	}, ; 110: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10901304; uint32_t buffer_offset
	}, ; 111: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10915640; uint32_t buffer_offset
	}, ; 112: System.Net.Http.Json
	%struct.CompressedAssemblyDescriptor {
		i32 125952, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10927928; uint32_t buffer_offset
	}, ; 113: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11053880; uint32_t buffer_offset
	}, ; 114: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11092792; uint32_t buffer_offset
	}, ; 115: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11099960; uint32_t buffer_offset
	}, ; 116: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11105080; uint32_t buffer_offset
	}, ; 117: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 72704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11123512; uint32_t buffer_offset
	}, ; 118: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 396288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11196216; uint32_t buffer_offset
	}, ; 119: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11592504; uint32_t buffer_offset
	}, ; 120: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11601720; uint32_t buffer_offset
	}, ; 121: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 79360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11606840; uint32_t buffer_offset
	}, ; 122: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11686200; uint32_t buffer_offset
	}, ; 123: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 123904, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11700536; uint32_t buffer_offset
	}, ; 124: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11824440; uint32_t buffer_offset
	}, ; 125: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 388608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11854136; uint32_t buffer_offset
	}, ; 126: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 330752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12242744; uint32_t buffer_offset
	}, ; 127: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12573496; uint32_t buffer_offset
	}, ; 128: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12578616; uint32_t buffer_offset
	}, ; 129: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12590904; uint32_t buffer_offset
	}, ; 130: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12596024; uint32_t buffer_offset
	}, ; 131: System
	%struct.CompressedAssemblyDescriptor {
		i32 2024448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12601144; uint32_t buffer_offset
	}, ; 132: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 171008, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14625592; uint32_t buffer_offset
	}, ; 133: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22560, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14796600; uint32_t buffer_offset
	}, ; 134: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2239488, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14819160; uint32_t buffer_offset
	} ; 135: Mono.Android
], align 4

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 17058648, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [17058648 x i8] zeroinitializer, align 1

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
