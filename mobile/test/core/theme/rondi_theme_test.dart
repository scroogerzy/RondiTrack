import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';

void main() {
  test('status token pairs meet WCAG AA text contrast', () {
    for (final colors in [RondiStatusColors.light, RondiStatusColors.dark]) {
      final pairs = [
        (colors.openForeground, colors.openBackground),
        (colors.closedForeground, colors.closedBackground),
        (colors.paidOutForeground, colors.paidOutBackground),
        (colors.arrearsForeground, colors.arrearsBackground),
        (colors.contributionPaidForeground, colors.contributionPaidBackground),
        (
          colors.contributionPendingForeground,
          colors.contributionPendingBackground,
        ),
      ];
      for (final pair in pairs) {
        final a = pair.$1.computeLuminance();
        final b = pair.$2.computeLuminance();
        final lighter = a > b ? a : b;
        final darker = a > b ? b : a;
        expect((lighter + 0.05) / (darker + 0.05), greaterThanOrEqualTo(4.5));
      }
    }
  });
}
