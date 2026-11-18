import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_status.dart';

class StokvelCycleScreen extends ConsumerWidget {
  const new({required this.stokvelId, required this.cycleId, super.key});
  final String stokvelId;
  final String cycleId;
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detail = ref.watch(stokvelDetailProvider(stokvelId));
    return Scaffold(
      appBar: AppBar(
        title: const Text('Cycle'),
        leading: BackButton(onPressed: () => context.pop()),
      ),
      body: detail.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('We could not load this cycle.'),
              TextButton(
                onPressed: () =>
                    ref.invalidate(stokvelDetailProvider(stokvelId)),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (item) {
          if (item == null || item.currentCycle?.id != cycleId) {
            return const Center(child: Text('Cycle not found.'));
          }
          final cycle = item.currentCycle!;
          return ListView(
            padding: const EdgeInsets.all(20),
            children: [
              Text(
                'Cycle ${cycle.periodNumber}',
                style: Theme.of(context).textTheme.headlineMedium,
              ),
              Text('Status: ${CycleStatus.fromApi(cycle.status).label}'),
              Text('Collected: ${cycle.totalCollectedCents} cents'),
              const SizedBox(height: 16),
              Text(
                'Contributions',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              if (item.contributions.where((c) => c.cycleId == cycleId).isEmpty)
                const Text('No contributions yet.'),
              for (final contribution in item.contributions.where(
                (c) => c.cycleId == cycleId,
              ))
                ListTile(
                  title: Text(contribution.userId),
                  subtitle: Text(
                    contribution.recordedAtUtc.toLocal().toString(),
                  ),
                  trailing: Text('${contribution.amountCents} cents'),
                ),
            ],
          );
        },
      ),
    );
  }
}
