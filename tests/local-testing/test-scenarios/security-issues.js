// Security vulnerability test file for PR reviewer bot
// This file contains various security issues that the bot should detect and flag

/**
 * SECURITY ISSUE: Hardcoded credentials
 * Bot should flag these as critical security vulnerabilities
 */
const DATABASE_PASSWORD = "admin123"; // Hardcoded password - SECURITY RISK
const API_KEY = "sk-1234567890abcdef"; // Hardcoded API key - SECURITY RISK
const JWT_SECRET = "my-super-secret-key"; // Hardcoded JWT secret - SECURITY RISK

/**
 * SECURITY ISSUE: SQL Injection vulnerability
 * Bot should detect potential SQL injection
 */
function getUserById(userId) {
    // Direct string concatenation in SQL query - SQL INJECTION RISK
    const query = "SELECT * FROM users WHERE id = '" + userId + "'";
    
    // Simulated database call
    return executeQuery(query);
}

/**
 * SECURITY ISSUE: XSS vulnerability
 * Bot should flag unsafe HTML rendering
 */
function displayUserMessage(message) {
    // Direct HTML injection without sanitization - XSS RISK
    document.getElementById('messageDiv').innerHTML = message;
}

/**
 * SECURITY ISSUE: Insecure random number generation
 * Bot should suggest cryptographically secure alternatives
 */
function generateToken() {
    // Math.random() is not cryptographically secure - SECURITY RISK
    return Math.random().toString(36).substring(2, 15);
}

/**
 * SECURITY ISSUE: Weak password validation
 * Bot should suggest stronger password requirements
 */
function validatePassword(password) {
    // Very weak password validation - SECURITY RISK
    return password.length >= 4;
}

/**
 * SECURITY ISSUE: Insecure file path handling
 * Bot should detect path traversal vulnerability
 */
function readUserFile(filename) {
    const fs = require('fs');
    
    // No path sanitization - PATH TRAVERSAL RISK
    const filePath = './uploads/' + filename;
    
    return fs.readFileSync(filePath, 'utf8');
}

/**
 * SECURITY ISSUE: Unsafe eval usage
 * Bot should flag eval as dangerous
 */
function executeUserCode(code) {
    // eval() is extremely dangerous - SECURITY RISK
    return eval(code);
}

/**
 * SECURITY ISSUE: Insecure HTTP requests
 * Bot should suggest HTTPS and certificate validation
 */
function fetchUserData(userId) {
    const http = require('http');
    
    // HTTP instead of HTTPS - SECURITY RISK
    const url = `http://api.example.com/users/${userId}`;
    
    // No certificate validation mentioned
    return http.get(url);
}

/**
 * SECURITY ISSUE: Information disclosure in error messages
 * Bot should flag detailed error exposure
 */
function authenticateUser(username, password) {
    try {
        // Simulated authentication
        if (username === 'admin' && password === DATABASE_PASSWORD) {
            return { success: true, user: { id: 1, username: 'admin' } };
        }
        throw new Error('Invalid credentials');
    } catch (error) {
        // Exposing internal error details - INFORMATION DISCLOSURE RISK
        return { 
            success: false, 
            error: error.message,
            stack: error.stack,
            databaseConnection: 'mysql://admin:admin123@localhost:3306/mydb'
        };
    }
}

module.exports = {
    getUserById,
    displayUserMessage,
    generateToken,
    validatePassword,
    readUserFile,
    executeUserCode,
    fetchUserData,
    authenticateUser
};

