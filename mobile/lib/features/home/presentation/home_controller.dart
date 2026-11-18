import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
part 'home_controller.g.dart';

@riverpod
class HomeController extends _$HomeController {
  @override
  Future<HomeResponse> build() => ref.watch(homeRepositoryProvider).loadHome();

  Future<void> refresh() async {
    final previousData = state.asData?.value;
    if (previousData != null) state = AsyncData(previousData);
    try {
      state = AsyncData(await ref.read(homeRepositoryProvider).loadHome());
    } on Object catch (error, stack) {
      if (previousData == null) {
        state = AsyncError(error, stack);
      } else {
        state = AsyncData(previousData);
      }
    }
  }
}

@riverpod
Future<StokvelHomeItem?> stokvelDetail(Ref ref, String stokvelId) async {
  final item = await ref.watch(homeRepositoryProvider).loadStokvel(stokvelId);
  // A missing membership and an unknown id deliberately share the same result.
  return item;
}
