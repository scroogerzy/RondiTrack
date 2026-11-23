import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/failure.dart';
import '../../../core/network/rondi_api.dart';
import '../domain/activity_item.dart';
import 'activity_repository.dart';

class ApiActivityRepository implements ActivityRepository {
  ApiActivityRepository(this.api);
  final RondiApi api;

  @override
  Future<List<ActivityItem>> loadActivity() async {
    try {
      final page = await api.activity();
      return (page['items'] as List? ?? [])
          .whereType<Map>()
          .map((raw) {
        final item = Map<String, dynamic>.from(raw);
        return ActivityItem(
          title: item['summary'] as String? ?? '',
          detail: item['stokvelName'] as String? ?? '',
          when: DateTime.parse(item['occurredAtUtc'].toString()).toUtc(),
        );
      }).toList();
    } catch (error) {
      throw mapFailure(error);
    }
  }
}

final apiActivityRepositoryProvider = Provider<ApiActivityRepository>(
  (ref) => ApiActivityRepository(ref.watch(rondiApiProvider)),
);

final apiActivityFeedProvider = FutureProvider((ref) {
  return ref.watch(apiActivityRepositoryProvider).loadActivity();
});

