import 'package:ronditrack_mobile/features/home/domain/home_models.dart';

/// Cycle data follows ContributionCycleResponse from the API.
class StokvelCycle {
  const new({
    required this.id,
    required this.periodNumber,
    required this.startDateUtc,
    required this.endDateUtc,
    required this.targetAmountCents,
    required this.status,
    required this.totalCollectedCents,
    required this.amountDueCents,
    this.closedAtUtc,
    this.payoutProcessedAtUtc,
  });

  final String id;
  final int periodNumber;
  final DateTime startDateUtc;
  final DateTime endDateUtc;
  final int targetAmountCents;
  final String status;
  final int totalCollectedCents;
  final int amountDueCents;
  final DateTime? closedAtUtc;
  final DateTime? payoutProcessedAtUtc;
}

/// ContributionResponse has no status field. Keep only fields the API supplies.
class StokvelContribution {
  const new({
    required this.id,
    required this.userId,
    required this.cycleId,
    required this.amountCents,
    required this.recordedAtUtc,
  });

  final String id;
  final String userId;
  final String cycleId;
  final int amountCents;
  final DateTime recordedAtUtc;
}

class StokvelDetail {
  const new({
    required this.homeItem,
    required this.currentUserId,
    required this.currentCycle,
    required this.contributions,
  });

  final StokvelHomeItem homeItem;
  final String currentUserId;
  final StokvelCycle? currentCycle;
  final List<StokvelContribution> contributions;

  bool get hasOwnContribution =>
      contributions.any((contribution) => contribution.userId == currentUserId);

  int get ownContributionCents => contributions
      .where((contribution) => contribution.userId == currentUserId)
      .fold(0, (total, contribution) => total + contribution.amountCents);
}
