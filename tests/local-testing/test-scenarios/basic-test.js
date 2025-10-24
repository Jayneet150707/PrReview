// Basic JavaScript test file for PR reviewer bot testing
// This file contains simple code that should trigger basic review feedback

/**
 * Simple calculator function
 * The bot should provide feedback on documentation and error handling
 */
function calculator(a, b, operation) {
    // Missing input validation - bot should catch this
    if (operation === 'add') {
        return a + b;
    } else if (operation === 'subtract') {
        return a - b;
    } else if (operation === 'multiply') {
        return a * b;
    } else if (operation === 'divide') {
        // Potential division by zero - bot should flag this
        return a / b;
    }
    // Missing return statement for invalid operation - bot should catch this
}

/**
 * User greeting function
 * Tests basic code quality feedback
 */
function greetUser(name) {
    // String concatenation instead of template literals - bot should suggest improvement
    console.log("Hello, " + name + "!");
    
    // Unused variable - bot should flag this
    var unusedVariable = "This is not used";
    
    // Missing return statement
}

/**
 * Array processing function
 * Tests algorithm efficiency feedback
 */
function processArray(arr) {
    var result = [];
    
    // Inefficient nested loop - bot should suggest optimization
    for (var i = 0; i < arr.length; i++) {
        for (var j = 0; j < arr.length; j++) {
            if (i !== j && arr[i] === arr[j]) {
                result.push(arr[i]);
            }
        }
    }
    
    return result;
}

/**
 * Simple validation function
 * Tests error handling patterns
 */
function validateEmail(email) {
    // Very basic regex - bot should suggest more robust validation
    var emailRegex = /\S+@\S+\.\S+/;
    
    // No error handling - bot should suggest try-catch
    return emailRegex.test(email);
}

// Global variable usage - bot should flag this as bad practice
var globalCounter = 0;

/**
 * Counter function using global state
 * Tests state management feedback
 */
function incrementCounter() {
    globalCounter++;
    console.log("Counter: " + globalCounter);
}

// Export statements for testing
module.exports = {
    calculator,
    greetUser,
    processArray,
    validateEmail,
    incrementCounter
};

