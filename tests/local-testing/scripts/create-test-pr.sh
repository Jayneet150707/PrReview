#!/bin/bash

# Automated script to create test PRs for the PR reviewer bot
# This script creates different types of test PRs to validate bot functionality

set -e

echo "🧪 PR Reviewer Bot - Automated Testing Script"
echo "=============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_BRANCH="beta"
TEST_SCENARIOS_DIR="tests/local-testing/test-scenarios"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to create a test PR
create_test_pr() {
    local test_type=$1
    local test_file=$2
    local branch_name="test-${test_type}-$(date +%s)"
    local commit_message="Add ${test_type} test for PR reviewer bot"
    
    print_status "Creating test PR for: ${test_type}"
    
    # Create and checkout new branch
    git checkout -b "$branch_name" "$BASE_BRANCH"
    
    # Copy test file to root
    cp "$TEST_SCENARIOS_DIR/$test_file" "./test-${test_type}.js"
    
    # Add and commit
    git add "./test-${test_type}.js"
    git commit -m "$commit_message"
    
    # Push branch
    git push origin "$branch_name"
    
    print_success "Created branch: $branch_name"
    print_warning "Now create a PR manually targeting '$BASE_BRANCH' branch"
    
    # Return to base branch
    git checkout "$BASE_BRANCH"
    
    echo ""
}

# Function to show menu
show_menu() {
    echo "Select test scenario to create:"
    echo "1) Basic code review test"
    echo "2) Security issues test"
    echo "3) Performance issues test"
    echo "4) All test scenarios"
    echo "5) Exit"
    echo ""
}

# Main execution
main() {
    while true; do
        show_menu
        read -p "Enter your choice (1-5): " choice
        
        case $choice in
            1)
                create_test_pr "basic-review" "basic-test.js"
                ;;
            2)
                create_test_pr "security-issues" "security-issues.js"
                ;;
            3)
                create_test_pr "performance-issues" "performance-issues.js"
                ;;
            4)
                print_status "Creating all test scenarios..."
                create_test_pr "basic-review" "basic-test.js"
                sleep 2
                create_test_pr "security-issues" "security-issues.js"
                sleep 2
                create_test_pr "performance-issues" "performance-issues.js"
                print_success "All test PRs created!"
                ;;
            5)
                print_status "Exiting..."
                exit 0
                ;;
            *)
                print_error "Invalid choice. Please select 1-5."
                ;;
        esac
        
        echo ""
        read -p "Press Enter to continue or Ctrl+C to exit..."
        echo ""
    done
}

# Run main function
main

