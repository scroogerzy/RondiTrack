import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/features/activity/data/activity_repository.dart';

class ActivityScreen extends ConsumerWidget {
  const new({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activity = ref.watch(activityFeedProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Activity')),
      body: activity.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('We could not load activity.'),
              TextButton(
                onPressed: () => ref.invalidate(activityFeedProvider),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (items) => items.isEmpty
            ? const Center(child: Text('No activity yet.'))
            : ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: items.length,
                itemBuilder: (context, index) {
                  final item = items[index];
                  return Card(
                    child: ListTile(
                      leading: const Icon(Icons.history),
                      title: Text(item.title),
                      subtitle: Text(item.detail),
                      trailing: Text(
                        item.when.toLocal().toString().split(' ').first,
                      ),
                    ),
                  );
                },
              ),
      ),
    );
  }
}
