// Performance issues test file for PR reviewer bot
// This file contains various performance problems that the bot should detect

/**
 * PERFORMANCE ISSUE: Inefficient nested loops
 * Bot should suggest optimization (O(n²) to O(n) or O(n log n))
 */
function findDuplicates(arr) {
    const duplicates = [];
    
    // O(n²) complexity - PERFORMANCE ISSUE
    for (let i = 0; i < arr.length; i++) {
        for (let j = i + 1; j < arr.length; j++) {
            if (arr[i] === arr[j] && !duplicates.includes(arr[i])) {
                duplicates.push(arr[i]);
            }
        }
    }
    
    return duplicates;
}

/**
 * PERFORMANCE ISSUE: Inefficient string concatenation
 * Bot should suggest using array join or template literals
 */
function buildLargeString(items) {
    let result = "";
    
    // String concatenation in loop - PERFORMANCE ISSUE
    for (let i = 0; i < items.length; i++) {
        result += items[i] + ", ";
    }
    
    return result;
}

/**
 * PERFORMANCE ISSUE: Unnecessary DOM queries in loop
 * Bot should suggest caching DOM elements
 */
function updateMultipleElements(data) {
    // DOM query in loop - PERFORMANCE ISSUE
    for (let i = 0; i < data.length; i++) {
        document.getElementById('item-' + i).innerHTML = data[i];
        document.getElementById('item-' + i).style.color = 'red';
        document.getElementById('item-' + i).classList.add('updated');
    }
}

/**
 * PERFORMANCE ISSUE: Inefficient recursive function
 * Bot should suggest memoization or iterative approach
 */
function fibonacci(n) {
    // Exponential time complexity - PERFORMANCE ISSUE
    if (n <= 1) return n;
    return fibonacci(n - 1) + fibonacci(n - 2);
}

/**
 * PERFORMANCE ISSUE: Blocking synchronous operations
 * Bot should suggest async alternatives
 */
function processFiles(filePaths) {
    const results = [];
    
    // Synchronous file operations - PERFORMANCE ISSUE
    filePaths.forEach(path => {
        const fs = require('fs');
        const content = fs.readFileSync(path, 'utf8'); // Blocking operation
        const processed = content.toUpperCase();
        results.push(processed);
    });
    
    return results;
}

module.exports = {
    findDuplicates,
    buildLargeString,
    updateMultipleElements,
    fibonacci,
    processFiles
};

