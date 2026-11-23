import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';
import 'package:ronditrack_mobile/features/activity/domain/activity_item.dart';
part 'activity_repository.g.dart';

abstract interface class ActivityRepository {
  Future<List<ActivityItem>> loadActivity();
}

class FakeActivityRepository implements ActivityRepository {
  const new(this.email);
  final String? email;
  @override
  Future<List<ActivityItem>> loadActivity() async {
    if (email == null) return const [];
    return [
      ActivityItem(
        title: 'Contribution recorded',
        detail: 'Ubuntu Savings Club',
        when: DateTime.utc(2026, 11, 6),
      ),
      ActivityItem(
        title: 'Cycle opened',
        detail: 'Ubuntu Savings Club · Cycle 5',
        when: DateTime.utc(2026, 11),
      ),
      if (email == 'lerato@example.com')
        ActivityItem(
          title: 'Membership role',
          detail: 'Treasurer · Mzanzi Monthly Stokvel',
          when: DateTime.utc(2026, 10, 28),
        ),
    ];
  }
}

@Riverpod(keepAlive: true)
ActivityRepository activityRepository(Ref ref) =>
    FakeActivityRepository(ref.watch(fakeAuthProvider).email);

@riverpod
Future<List<ActivityItem>> activityFeed(Ref ref) =>
    ref.watch(activityRepositoryProvider).loadActivity();
