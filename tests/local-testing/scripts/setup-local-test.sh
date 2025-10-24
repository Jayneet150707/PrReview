#!/bin/bash

# Setup script for local testing of PR reviewer bot
# This script helps configure the local environment for testing

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

print_header() {
    echo ""
    echo "🤖 PR Reviewer Bot - Local Testing Setup"
    echo "========================================"
    echo ""
}

# Function to setup environment file
setup_env_file() {
    print_status "Setting up environment configuration..."
    
    if [ ! -f ".env" ]; then
        if [ -f ".env.example" ]; then
            cp .env.example .env
            print_success "Created .env file from .env.example"
        else
            cat > .env << EOF
# GitHub Configuration
GITHUB_TOKEN=your_github_personal_access_token_here
GITHUB_WEBHOOK_SECRET=your_secure_random_string_here
TARGET_BRANCH=beta

# OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_MODEL=gpt-4

# Server Configuration
PORT=3000
NODE_ENV=development
LOG_LEVEL=debug
EOF
            print_success "Created basic .env file"
        fi
        
        print_warning "⚠️  IMPORTANT: Edit .env file with your actual API keys!"
    else
        print_success ".env file already exists"
    fi
}

# Main execution
main() {
    print_header
    
    setup_env_file
    
    if [ -f "package.json" ]; then
        npm install
        print_success "Dependencies installed successfully"
    fi
    
    # Make scripts executable
    chmod +x tests/local-testing/scripts/*.sh
    print_success "Made test scripts executable"
    
    echo ""
    print_success "🎉 Local testing setup complete!"
    echo ""
    echo "Next steps:"
    echo "1. Edit .env file with your API keys"
    echo "2. Start the bot: npm run dev"
    echo "3. In another terminal, start ngrok: ngrok http 3000"
    echo "4. Configure GitHub webhook with ngrok URL"
    echo "5. Run test script: ./tests/local-testing/scripts/create-test-pr.sh"
}

# Run main function
main

