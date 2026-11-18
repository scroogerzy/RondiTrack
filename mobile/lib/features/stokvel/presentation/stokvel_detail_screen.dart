// Keep long UI expressions readable; avoid awkward line breaks.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:ronditrack_mobile/core/layout/window_class.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/navigation/app_routes.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_detail.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_status.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvels_screen.dart';

class StokvelDetailScreen extends ConsumerWidget {
  const new({required this.stokvelId, super.key});
  final String stokvelId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final widthClass = windowClassForWidth(MediaQuery.sizeOf(context).width);
    final detail = ref.watch(stokvelDetailProvider(stokvelId));
    const listPane = StokvelsScreen(showAppBar: false);
    final detailPane = _detailBody(context, ref, detail);
    if (widthClass == WindowClass.expanded) {
      return Scaffold(
        appBar: AppBar(title: const Text('Stokvels')),
        body: Row(
          children: [
            const SizedBox(width: 340, child: listPane),
            const VerticalDivider(width: 1),
            Expanded(child: detailPane),
          ],
        ),
      );
    }
    return Scaffold(
      appBar: AppBar(
        title: const Text('Stokvel details'),
        leading: BackButton(onPressed: () => context.pop()),
        actions: [
          PopupMenuButton<String>(
            tooltip: 'Switch stokvel',
            icon: const Icon(Icons.swap_horiz),
            onSelected: (id) => StokvelDetailRoute(id: id).go(context),
            itemBuilder: (context) => [
              for (final StokvelHomeItem item
                  in ref.watch(homeControllerProvider).asData?.value.stokvels ??
                      const <StokvelHomeItem>[])
                PopupMenuItem<String>(
                  value: item.stokvelId,
                  child: Text(item.name),
                ),
            ],
          ),
        ],
      ),
      body: detailPane,
    );
  }

  Widget _detailBody(
    BuildContext context,
    WidgetRef ref,
    AsyncValue<StokvelDetail?> detail,
  ) => detail.when(
    loading: () => const Center(child: CircularProgressIndicator()),
    error: (error, stack) => Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Text('We could not load this stokvel.'),
          TextButton(
            onPressed: () => ref.invalidate(stokvelDetailProvider(stokvelId)),
            child: const Text('Retry'),
          ),
        ],
      ),
    ),
    data: (item) {
      if (item == null) return const Center(child: Text('Stokvel not found.'));
      final role = StokvelRole.fromApi(item.homeItem.role);
      final actions = availableActions(
        role: role,
        cycleStatus: CycleStatus.fromApi(item.currentCycle?.status ?? ''),
        contributionStatus: ContributionStatus.unknown,
        hasOwnContribution: item.hasOwnContribution,
      );
      final statusColors =
          Theme.of(context).extension<RondiStatusColors>() ??
          RondiStatusColors.light;
      return ListView(
        padding: const EdgeInsets.all(20),
        children: [
          Text(
            item.homeItem.name,
            style: Theme.of(context).textTheme.headlineMedium,
          ),
          Text('Your role: ${item.homeItem.role}'),
          const SizedBox(height: 16),
          const Text('Current cycle'),
          if (item.currentCycle case final cycle?)
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Cycle ${cycle.periodNumber}',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    Builder(
                      builder: (context) {
                        final status = CycleStatus.fromApi(cycle.status);
                        final pair = switch (status) {
                          CycleStatus.open => (
                            statusColors.openBackground,
                            statusColors.openForeground,
                          ),
                          CycleStatus.closed => (
                            statusColors.closedBackground,
                            statusColors.closedForeground,
                          ),
                          CycleStatus.payoutProcessed => (
                            statusColors.paidOutBackground,
                            statusColors.paidOutForeground,
                          ),
                          CycleStatus.scheduled || CycleStatus.unknown => (
                            Theme.of(context)
                                .colorScheme
                                .surfaceContainerHighest,
                            Theme.of(context).colorScheme.onSurface,
                          ),
                        };
                        return Chip(
                          label: Text(status.label),
                          backgroundColor: pair.$1,
                          labelStyle: TextStyle(color: pair.$2),
                        );
                      },
                    ),
                    Text(
                      'Collected: ${Cents(cycle.totalCollectedCents).formatEnZa()}',
                    ),
                    Text(
                      'Amount due: ${Cents(cycle.amountDueCents).formatEnZa()}',
                    ),
                    FilledButton.tonal(
                      onPressed: () => StokvelCycleRoute(
                        id: stokvelId,
                        cycleId: cycle.id,
                      ).push<void>(context),
                      child: const Text('View cycle'),
                    ),
                  ],
                ),
              ),
            )
          else
            const Text('No current cycle.'),
          const SizedBox(height: 16),
          Text('Contributions', style: Theme.of(context).textTheme.titleMedium),
          if (item.contributions.isEmpty) const Text('No contributions yet.'),
          for (final contribution in item.contributions)
            ListTile(
              title: Text(contribution.userId),
              subtitle: Text(
                contribution.recordedAtUtc
                    .toLocal()
                    .toString()
                    .split(' ')
                    .first,
              ),
              trailing: Text(Cents(contribution.amountCents).formatEnZa()),
            ),
          const SizedBox(height: 16),
          Text(
            'Available actions',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          if (actions.isEmpty)
            const Text(
              'No actions are available for this membership and cycle.',
            ),
          for (final action in actions)
            ListTile(
              title: Text(action.label),
              leading: const Icon(Icons.lock_outline),
              subtitle: const Text('Preview only in the fake-data assignment.'),
            ),
        ],
      );
    },
  );
}
