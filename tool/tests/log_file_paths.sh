# Defines constants that represent file paths in log repository
# structure generated when running ekstazi tool
# Paths are defined relatively to the root of the log directory

readonly EXECUTED_TEST_METHODS_COUNT_FILE="executionLogs/executed_test_methods_count.txt"
readonly PASSED_TEST_METHODS_COUNT_FILE="executionLogs/passed_test_methods_count.txt"
readonly SKIPPED_TEST_METHODS_COUNT_FILE="executionLogs/skipped_test_methods_count.txt"
readonly TOTAL_TEST_METHODS_COUNT_FILE="executionLogs/total_test_methods_count.txt"

readonly EXECUTED_TEST_CLASSES_COUNT_FILE="executionLogs/executed_test_classes_count.txt"
readonly TOTAL_TEST_CLASSES_COUNT_FILE="executionLogs/total_test_classes_count.txt"

readonly ANALYSIS_TIME_FILE="executionLogs/analysis_time.txt"
readonly INSTRUMENTATION_TIME_FILE="executionLogs/instrumentation_time.txt"
readonly EXECUTION_TIME_FILE="executionLogs/execution_time.txt"
readonly TOTAL_TIME_FILE="executionLogs/total_time.txt"

file_names=(
    "EXECUTED_TEST_METHODS_COUNT_FILE"
    "PASSED_TEST_METHODS_COUNT_FILE"
    "SKIPPED_TEST_METHODS_COUNT_FILE"
    "TOTAL_TEST_METHODS_COUNT_FILE"
    "EXECUTED_TEST_CLASSES_COUNT_FILE"
    "TOTAL_TEST_CLASSES_COUNT_FILE"
    "ANALYSIS_TIME_FILE"
    "INSTRUMENTATION_TIME_FILE"
    "EXECUTION_TIME_FILE"
    "TOTAL_TIME_FILE"
)
