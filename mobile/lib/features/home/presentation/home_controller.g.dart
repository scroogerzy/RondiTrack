// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'home_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(HomeController)
final homeControllerProvider = HomeControllerProvider._();

final class HomeControllerProvider
    extends $AsyncNotifierProvider<HomeController, HomeResponse> {
  HomeControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'homeControllerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$homeControllerHash();

  @$internal
  @override
  HomeController create() => HomeController();
}

String _$homeControllerHash() => r'25f6ed18b801c882ebac7eab180ec47526635c6d';

abstract class _$HomeController extends $AsyncNotifier<HomeResponse> {
  FutureOr<HomeResponse> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<HomeResponse>, HomeResponse>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<HomeResponse>, HomeResponse>,
              AsyncValue<HomeResponse>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}

@ProviderFor(stokvelDetail)
final stokvelDetailProvider = StokvelDetailFamily._();

final class StokvelDetailProvider
    extends
        $FunctionalProvider<
          AsyncValue<StokvelHomeItem?>,
          StokvelHomeItem?,
          FutureOr<StokvelHomeItem?>
        >
    with $FutureModifier<StokvelHomeItem?>, $FutureProvider<StokvelHomeItem?> {
  StokvelDetailProvider._({
    required StokvelDetailFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'stokvelDetailProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$stokvelDetailHash();

  @override
  String toString() {
    return r'stokvelDetailProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<StokvelHomeItem?> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<StokvelHomeItem?> create(Ref ref) {
    final argument = this.argument as String;
    return stokvelDetail(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is StokvelDetailProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$stokvelDetailHash() => r'73a82bd6f08f48873ffc0c0c181c90b0efed7995';

final class StokvelDetailFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<StokvelHomeItem?>, String> {
  StokvelDetailFamily._()
    : super(
        retry: null,
        name: r'stokvelDetailProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  StokvelDetailProvider call(String stokvelId) =>
      StokvelDetailProvider._(argument: stokvelId, from: this);

  @override
  String toString() => r'stokvelDetailProvider';
}
