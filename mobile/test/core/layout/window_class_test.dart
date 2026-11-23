import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/layout/window_class.dart';

void main() {
  test('window breakpoints use available width', () {
    expect(windowClassForWidth(599), WindowClass.compact);
    expect(windowClassForWidth(600), WindowClass.medium);
    expect(windowClassForWidth(839), WindowClass.medium);
    expect(windowClassForWidth(840), WindowClass.expanded);
  });
}
