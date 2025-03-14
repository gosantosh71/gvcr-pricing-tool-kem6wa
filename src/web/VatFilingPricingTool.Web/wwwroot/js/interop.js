/**
 * interop.js - JavaScript interoperability for VatFilingPricingTool
 * Provides browser-specific functionality to the Blazor WebAssembly application
 * Version: 1.0.0
 */

// Import Chart.js - version 3.7.0
// Note: This import assumes Chart.js is included via a <script> tag in the HTML
// or bundled separately, as ES6 imports may not work directly in this context

// Global object to store Chart.js instances
window.chartInstances = {};

/**
 * Initializes a Chart.js chart on a specified canvas element
 * @param {string} elementId - The ID of the canvas element
 * @param {object} chartData - The data to display in the chart
 * @param {object} chartOptions - The options for configuring the chart
 * @returns {object} Chart instance that can be referenced later
 */
window.initializeChart = function(elementId, chartData, chartOptions) {
    // Get the canvas element by ID
    const canvas = document.getElementById(elementId);
    if (!canvas) {
        console.error(`Canvas element with ID '${elementId}' not found`);
        return null;
    }

    // Check if a chart instance already exists for this element and destroy it if found
    if (window.chartInstances[elementId]) {
        window.chartInstances[elementId].destroy();
        delete window.chartInstances[elementId];
    }

    // Create a new Chart instance with the provided data and options
    try {
        const ctx = canvas.getContext('2d');
        const chartInstance = new Chart(ctx, {
            type: chartData.type || 'bar',
            data: chartData,
            options: chartOptions || {}
        });

        // Store the chart instance in the global chartInstances object
        window.chartInstances[elementId] = chartInstance;

        // Return the chart instance for reference
        return chartInstance;
    } catch (error) {
        console.error(`Error initializing chart: ${error}`);
        return null;
    }
};

/**
 * Updates an existing chart with new data
 * @param {string} elementId - The ID of the canvas element
 * @param {object} newData - The new data to display in the chart
 */
window.updateChart = function(elementId, newData) {
    // Check if the chart instance exists
    const chartInstance = window.chartInstances[elementId];
    if (!chartInstance) {
        console.error(`Chart instance for element ID '${elementId}' not found`);
        return;
    }

    // Update the chart data with the new data
    chartInstance.data = newData;
    
    // Call chart.update() to refresh the visualization
    chartInstance.update();
};

/**
 * Destroys a Chart.js instance and removes it from the global registry
 * @param {string} elementId - The ID of the canvas element
 * @returns {boolean} True if chart was successfully destroyed, false otherwise
 */
window.destroyChart = function(elementId) {
    // Get the chart instance from the chartInstances object
    const chartInstance = window.chartInstances[elementId];
    if (!chartInstance) {
        console.warn(`Chart instance for element ID '${elementId}' not found`);
        return false;
    }

    // Call chart.destroy() to clean up resources
    chartInstance.destroy();
    
    // Remove the chart from the chartInstances object
    delete window.chartInstances[elementId];
    
    // Return true to indicate success
    return true;
};

/**
 * Triggers a file download in the browser
 * @param {string} fileName - The name to give the downloaded file
 * @param {string} base64Content - The file content as a base64 encoded string
 * @param {string} contentType - The MIME type of the file
 */
window.downloadFile = function(fileName, base64Content, contentType) {
    try {
        // Convert base64 content to a Blob with the specified content type
        const byteCharacters = atob(base64Content);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);
            
            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }
            
            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        const blob = new Blob(byteArrays, { type: contentType });
        
        // Create a URL for the Blob using URL.createObjectURL
        const url = URL.createObjectURL(blob);
        
        // Create a temporary anchor element
        const link = document.createElement('a');
        link.href = url;
        
        // Set the download attribute to the fileName
        link.download = fileName;
        
        // Append the anchor to the document
        document.body.appendChild(link);
        
        // Trigger a click on the anchor to start the download
        link.click();
        
        // Remove the anchor from the document
        document.body.removeChild(link);
        
        // Revoke the Blob URL to free up memory
        setTimeout(() => URL.revokeObjectURL(url), 100);
    } catch (error) {
        console.error(`Error downloading file: ${error}`);
    }
};

/**
 * Displays a toast notification message to the user
 * @param {string} message - The message to display
 * @param {string} type - The type of message (success, error, warning, info)
 * @param {number} durationMs - The duration to show the toast in milliseconds
 */
window.showToast = function(message, type, durationMs) {
    try {
        // Create a toast element with the appropriate styling based on type
        const toast = document.createElement('div');
        toast.className = `vat-toast vat-toast-${type || 'info'}`;
        toast.style.position = 'fixed';
        toast.style.bottom = '20px';
        toast.style.right = '20px';
        toast.style.zIndex = '9999';
        toast.style.padding = '12px 20px';
        toast.style.borderRadius = '4px';
        toast.style.boxShadow = '0 4px 8px rgba(0, 0, 0, 0.2)';
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s ease-in-out';
        
        // Set colors based on type
        switch (type) {
            case 'success':
                toast.style.backgroundColor = '#4caf50';
                toast.style.color = 'white';
                break;
            case 'error':
                toast.style.backgroundColor = '#f44336';
                toast.style.color = 'white';
                break;
            case 'warning':
                toast.style.backgroundColor = '#ff9800';
                toast.style.color = 'white';
                break;
            case 'info':
            default:
                toast.style.backgroundColor = '#2196f3';
                toast.style.color = 'white';
                break;
        }
        
        // Set the message text content
        toast.textContent = message;
        
        // Append the toast to the document body
        document.body.appendChild(toast);
        
        // Add a CSS class to trigger the show animation
        setTimeout(() => toast.style.opacity = '1', 10);
        
        // Set a timeout to remove the toast after the specified duration
        const duration = durationMs || 3000;
        
        // Fade out before removing
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => {
                if (toast.parentNode) {
                    document.body.removeChild(toast);
                }
            }, 300);
        }, duration);
        
        // Add a click handler to dismiss the toast early
        toast.addEventListener('click', () => {
            toast.style.opacity = '0';
            setTimeout(() => {
                if (toast.parentNode) {
                    document.body.removeChild(toast);
                }
            }, 300);
        });
    } catch (error) {
        console.error(`Error showing toast: ${error}`);
    }
};

/**
 * Scrolls the page to bring a specific element into view
 * @param {string} elementId - The ID of the element to scroll to
 * @param {boolean} smooth - Whether to use smooth scrolling
 */
window.scrollToElement = function(elementId, smooth) {
    try {
        // Get the element by ID
        const element = document.getElementById(elementId);
        
        // If element exists, call element.scrollIntoView()
        if (element) {
            element.scrollIntoView({
                behavior: smooth ? 'smooth' : 'auto',
                block: 'start'
            });
        } else {
            console.warn(`Element with ID '${elementId}' not found for scrolling`);
        }
    } catch (error) {
        console.error(`Error scrolling to element: ${error}`);
    }
};

/**
 * Prints a specific element on the page
 * @param {string} elementId - The ID of the element to print
 */
window.printElement = function(elementId) {
    try {
        // Get the element by ID
        const element = document.getElementById(elementId);
        
        if (!element) {
            console.warn(`Element with ID '${elementId}' not found for printing`);
            return;
        }
        
        // Create a new window for printing
        const printWindow = window.open('', '_blank');
        if (!printWindow) {
            alert('Please allow popups for this website to enable printing.');
            return;
        }
        
        // Write the element's HTML content to the new window
        printWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>Print - VAT Filing Pricing Tool</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                    .print-header { text-align: center; margin-bottom: 20px; }
                    @media print {
                        .no-print { display: none; }
                        body { margin: 0; }
                    }
                </style>
            </head>
            <body>
                <div class="print-header">
                    <h1>VAT Filing Pricing Tool</h1>
                </div>
                ${element.outerHTML}
                <div class="no-print" style="text-align: center; margin-top: 20px;">
                    <button onclick="window.print();window.close();">Print</button>
                </div>
            </body>
            </html>
        `);
        
        // Add necessary styling to the new window
        printWindow.document.close();
        
        // Focus the print window
        printWindow.focus();
    } catch (error) {
        console.error(`Error printing element: ${error}`);
    }
};

/**
 * Stores a value in the browser's local storage
 * @param {string} key - The key to store the value under
 * @param {string} value - The value to store
 */
window.setLocalStorageItem = function(key, value) {
    try {
        // Use localStorage.setItem(key, value) to store the value
        localStorage.setItem(key, value);
    } catch (error) {
        console.error(`Error setting localStorage item: ${error}`);
    }
};

/**
 * Retrieves a value from the browser's local storage
 * @param {string} key - The key to retrieve the value for
 * @returns {string} The stored value or null if not found
 */
window.getLocalStorageItem = function(key) {
    try {
        // Use localStorage.getItem(key) to retrieve the value
        return localStorage.getItem(key);
    } catch (error) {
        console.error(`Error getting localStorage item: ${error}`);
        return null;
    }
};

/**
 * Removes an item from the browser's local storage
 * @param {string} key - The key to remove
 */
window.removeLocalStorageItem = function(key) {
    try {
        // Use localStorage.removeItem(key) to remove the item
        localStorage.removeItem(key);
    } catch (error) {
        console.error(`Error removing localStorage item: ${error}`);
    }
};

/**
 * Clears all items from the browser's local storage
 */
window.clearLocalStorage = function() {
    try {
        // Use localStorage.clear() to remove all items
        localStorage.clear();
    } catch (error) {
        console.error(`Error clearing localStorage: ${error}`);
    }
};

/**
 * Toggles between light and dark theme modes
 * @param {boolean} isDarkMode - Whether to enable dark mode
 */
window.toggleDarkMode = function(isDarkMode) {
    try {
        // Remove existing theme classes from document.body
        document.body.classList.remove('light-theme', 'dark-theme');
        
        // Add 'dark-theme' class if isDarkMode is true, otherwise add 'light-theme'
        document.body.classList.add(isDarkMode ? 'dark-theme' : 'light-theme');
        
        // Update theme-color meta tag for browser UI
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', isDarkMode ? '#1e1e1e' : '#ffffff');
        }
        
        // Store theme preference in local storage
        localStorage.setItem('darkMode', isDarkMode ? 'true' : 'false');
        
        // Update any chart instances to match the new theme
        updateChartsForTheme(isDarkMode);
    } catch (error) {
        console.error(`Error toggling dark mode: ${error}`);
    }
};

/**
 * Updates all charts to match the current theme
 * @param {boolean} isDarkMode - Whether dark mode is enabled
 */
function updateChartsForTheme(isDarkMode) {
    // Define colors for light and dark themes
    const textColor = isDarkMode ? '#ffffff' : '#333333';
    const gridColor = isDarkMode ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';
    
    // Update each chart instance
    Object.values(window.chartInstances).forEach(chart => {
        if (chart.options.scales) {
            // Update x-axis
            if (chart.options.scales.x) {
                chart.options.scales.x.ticks = chart.options.scales.x.ticks || {};
                chart.options.scales.x.ticks.color = textColor;
                chart.options.scales.x.grid = chart.options.scales.x.grid || {};
                chart.options.scales.x.grid.color = gridColor;
            }
            
            // Update y-axis
            if (chart.options.scales.y) {
                chart.options.scales.y.ticks = chart.options.scales.y.ticks || {};
                chart.options.scales.y.ticks.color = textColor;
                chart.options.scales.y.grid = chart.options.scales.y.grid || {};
                chart.options.scales.y.grid.color = gridColor;
            }
        }
        
        // Update legend
        if (chart.options.plugins && chart.options.plugins.legend) {
            chart.options.plugins.legend.labels = chart.options.plugins.legend.labels || {};
            chart.options.plugins.legend.labels.color = textColor;
        }
        
        // Update the chart
        chart.update();
    });
}

/**
 * Formats a number as a currency string with the specified currency code
 * @param {number} amount - The amount to format
 * @param {string} currencyCode - The ISO currency code (e.g., 'USD', 'EUR', 'GBP')
 * @returns {string} Formatted currency string
 */
window.formatCurrency = function(amount, currencyCode) {
    try {
        // Use Intl.NumberFormat with the appropriate locale and currency options
        const formatter = new Intl.NumberFormat(navigator.language, {
            style: 'currency',
            currency: currencyCode || 'EUR',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        
        // Format the amount with the specified currency code
        return formatter.format(amount);
    } catch (error) {
        console.error(`Error formatting currency: ${error}`);
        return amount.toString();
    }
};

// Initialization code that runs when the script is loaded
(function() {
    // Initialize the chartInstances object to store Chart.js instances if not already initialized
    window.chartInstances = window.chartInstances || {};
    
    // Check if Chart.js is available and log an error if not
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded. Please ensure Chart.js (version 3.7.0) is included before interop.js');
    }
    
    // Set up any global event listeners needed for interop functionality
    window.addEventListener('resize', function() {
        // Resize all charts when the window is resized
        Object.values(window.chartInstances).forEach(chart => {
            if (chart && typeof chart.resize === 'function') {
                chart.resize();
            }
        });
    });
    
    // Initialize theme based on stored preference
    const storedTheme = localStorage.getItem('darkMode');
    if (storedTheme === 'true') {
        window.toggleDarkMode(true);
    }
    
    console.log('VAT Filing Pricing Tool interop.js initialized');
})();