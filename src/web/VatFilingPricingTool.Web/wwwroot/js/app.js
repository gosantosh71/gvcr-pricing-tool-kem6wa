/**
 * VAT Filing Pricing Tool - Main Application JavaScript
 * 
 * This file contains the core client-side functionality for the VAT Filing Pricing Tool
 * web application, providing initialization, UI enhancements, and utility functions.
 * 
 * @version 1.0.0
 */

// Import Chart.js - v3.7.0
// Note: This will work if Chart.js is loaded via a script tag in the HTML
// or imported via a module bundler. We reference it here for documentation.

/**
 * Global namespace for the VAT Filing Pricing Tool application
 */
window.vatFilingApp = window.vatFilingApp || {};

/**
 * Object to store Chart.js instances for reference and management
 */
window.chartInstances = window.chartInstances || {};

/**
 * Object to store application settings and configuration
 */
window.appSettings = window.appSettings || {};

/**
 * Boolean flag indicating whether the application has been initialized
 */
window.appInitialized = false;

/**
 * Main application class that encapsulates the VAT Filing Pricing Tool functionality
 */
class VatFilingApp {
    /**
     * Initializes a new instance of the VatFilingApp class
     */
    constructor() {
        this.settings = {};
        this.chartInstances = {};
        this.initialized = false;
        this.currentTheme = 'light';
    }

    /**
     * Initializes the application
     * @returns {Promise<void>} Promise that resolves when initialization is complete
     */
    async initialize() {
        try {
            // Load application settings
            this.settings = await loadAppSettings();
            
            // Set up event listeners
            this.setupEventListeners();
            
            // Configure theme
            this.configureTheme();
            
            // Set up Chart.js defaults
            this.setupChartDefaults();
            
            // Register service worker
            this.registerServiceWorker();
            
            // Mark application as initialized
            this.initialized = true;
            window.appInitialized = true;
            
            console.log('VAT Filing Pricing Tool application initialized successfully');
        } catch (error) {
            console.error('Failed to initialize VAT Filing Pricing Tool application:', error);
        }
    }

    /**
     * Sets up event listeners for various UI interactions
     */
    setupEventListeners() {
        // Throttled window resize handler
        const throttledResize = throttle(this.handleWindowResize.bind(this), 250);
        window.addEventListener('resize', throttledResize);
        
        // Throttled scroll handler
        const throttledScroll = throttle(this.handleScroll.bind(this), 100);
        window.addEventListener('scroll', throttledScroll);
        
        // Set up theme toggle if it exists
        const themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => {
                const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
                this.applyTheme(newTheme);
            });
        }
        
        // Set up scroll-to-top button if it exists
        const scrollTopButton = document.getElementById('scroll-top-button');
        if (scrollTopButton) {
            scrollTopButton.addEventListener('click', scrollToTop);
        }
    }

    /**
     * Configures the application theme based on user preferences
     */
    configureTheme() {
        // Check for saved theme preference
        let theme = getLocalStorageItem('vat-filing-theme');
        
        // If no saved preference, check system preference
        if (!theme) {
            const prefersDarkMode = window.matchMedia && 
                window.matchMedia('(prefers-color-scheme: dark)').matches;
            theme = prefersDarkMode ? 'dark' : 'light';
        }
        
        // Apply theme
        this.applyTheme(theme);
    }

    /**
     * Applies the specified theme to the application
     * @param {string} theme - The theme to apply ('light' or 'dark')
     */
    applyTheme(theme) {
        // Remove existing theme classes
        document.body.classList.remove('light-theme', 'dark-theme');
        
        // Add new theme class
        document.body.classList.add(`${theme}-theme`);
        
        // Update theme-color meta tag for browser UI
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', 
                theme === 'dark' ? '#202124' : '#ffffff');
        }
        
        // Store theme preference
        setLocalStorageItem('vat-filing-theme', theme);
        
        // Update charts if they exist
        this.updateChartTheme(theme);
        
        // Update current theme
        this.currentTheme = theme;
        
        console.log(`Theme set to: ${theme}`);
    }

    /**
     * Sets up global defaults for Chart.js charts
     */
    setupChartDefaults() {
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js not found, skipping chart defaults setup');
            return;
        }
        
        // Set default colors based on current theme
        const isDarkTheme = this.currentTheme === 'dark';
        
        Chart.defaults.color = isDarkTheme ? '#e0e0e0' : '#666666';
        Chart.defaults.font.family = "'Segoe UI', 'Roboto', 'Helvetica Neue', Arial, sans-serif";
        
        // Default chart colors for the application
        const colors = isDarkTheme ? 
            ['#4285F4', '#34A853', '#FBBC05', '#EA4335', '#8AB4F8', '#81C995', '#FDE293', '#F28B82'] : 
            ['#1976D2', '#388E3C', '#FBC02D', '#D32F2F', '#0D47A1', '#1B5E20', '#F57F17', '#B71C1C'];
        
        // Set global options
        Chart.defaults.set('plugins', {
            legend: {
                position: 'top',
                labels: {
                    padding: 20,
                    usePointStyle: true,
                    pointStyle: 'circle'
                }
            },
            tooltip: {
                backgroundColor: isDarkTheme ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.8)',
                titleColor: isDarkTheme ? '#ffffff' : '#000000',
                bodyColor: isDarkTheme ? '#e0e0e0' : '#666666',
                borderWidth: 1,
                borderColor: isDarkTheme ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)',
                cornerRadius: 6,
                padding: 10,
                usePointStyle: true
            }
        });
        
        // Keep reference to colors for later use
        this.chartColors = colors;
        
        console.log('Chart.js defaults configured');
    }

    /**
     * Updates chart themes when the application theme changes
     * @param {string} theme - The current theme ('light' or 'dark')
     */
    updateChartTheme(theme) {
        const isDarkTheme = theme === 'dark';
        
        if (typeof Chart === 'undefined') {
            return;
        }
        
        // Update Chart.js default colors
        Chart.defaults.color = isDarkTheme ? '#e0e0e0' : '#666666';
        
        // Update tooltip styles
        Chart.defaults.set('plugins.tooltip', {
            backgroundColor: isDarkTheme ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.8)',
            titleColor: isDarkTheme ? '#ffffff' : '#000000',
            bodyColor: isDarkTheme ? '#e0e0e0' : '#666666',
            borderColor: isDarkTheme ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'
        });
        
        // Update each chart instance
        this.updateChartInstances();
    }

    /**
     * Updates all chart instances when needed (e.g., on theme change)
     */
    updateChartInstances() {
        // Check if Chart.js is available
        if (typeof Chart === 'undefined' || !window.chartInstances) {
            return;
        }
        
        // Update each chart with new theme colors
        for (const [id, chart] of Object.entries(window.chartInstances)) {
            if (chart && typeof chart.update === 'function') {
                // Call custom update function if it exists, otherwise just update the chart
                if (typeof window.updateChart === 'function') {
                    window.updateChart(id, chart);
                } else {
                    chart.update();
                }
            }
        }
    }

    /**
     * Registers a service worker for offline capabilities
     */
    registerServiceWorker() {
        if ('serviceWorker' in navigator) {
            window.addEventListener('load', () => {
                navigator.serviceWorker.register('/service-worker.js')
                    .then(registration => {
                        console.log('Service Worker registered with scope:', registration.scope);
                    })
                    .catch(error => {
                        console.error('Service Worker registration failed:', error);
                    });
            });
        }
    }

    /**
     * Handles window resize events
     */
    handleWindowResize() {
        // Get current viewport dimensions
        const width = window.innerWidth;
        const height = window.innerHeight;
        
        // Apply responsive classes based on screen size
        const body = document.body;
        body.classList.remove('xs-screen', 'sm-screen', 'md-screen', 'lg-screen', 'xl-screen');
        
        if (width < 576) {
            body.classList.add('xs-screen');
        } else if (width < 768) {
            body.classList.add('sm-screen');
        } else if (width < 992) {
            body.classList.add('md-screen');
        } else if (width < 1200) {
            body.classList.add('lg-screen');
        } else {
            body.classList.add('xl-screen');
        }
        
        // Update chart sizes if applicable
        this.updateChartInstances();
        
        // Custom event for components to react to resize
        const event = new CustomEvent('vatfilingsizechanged', { 
            detail: { width, height } 
        });
        window.dispatchEvent(event);
    }

    /**
     * Handles window scroll events
     */
    handleScroll() {
        // Get current scroll position
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        
        // Handle header behavior
        const header = document.querySelector('.app-header');
        if (header) {
            if (scrollTop > 50) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        }
        
        // Handle scroll-to-top button visibility
        const scrollTopButton = document.getElementById('scroll-top-button');
        if (scrollTopButton) {
            if (scrollTop > 300) {
                scrollTopButton.classList.add('visible');
            } else {
                scrollTopButton.classList.remove('visible');
            }
        }
        
        // Additional scroll-based animations can be added here
    }
}

/**
 * Initializes the VAT Filing Pricing Tool application
 * This is the main entry point called when the DOM is loaded
 */
function initializeApp() {
    // Create and initialize the application
    const app = new VatFilingApp();
    window.vatFilingApp = app;
    
    // Initialize the application
    app.initialize().then(() => {
        // Trigger an initial resize to set up responsive layout
        app.handleWindowResize();
        
        // Setup form validation if needed
        setupFormValidation();
        
        // Dispatch event that the application is ready
        window.dispatchEvent(new CustomEvent('vatfilingappready'));
    });
}

/**
 * Sets up client-side form validation
 */
function setupFormValidation() {
    // Find all forms that need validation
    const forms = document.querySelectorAll('form.needs-validation');
    
    Array.from(forms).forEach(form => {
        // Prevent submission if validation fails
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            
            form.classList.add('was-validated');
        }, false);
        
        // Real-time validation on input
        const inputs = form.querySelectorAll('input, select, textarea');
        Array.from(inputs).forEach(input => {
            input.addEventListener('blur', () => {
                if (input.checkValidity()) {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                } else {
                    input.classList.remove('is-valid');
                    input.classList.add('is-invalid');
                }
            });
        });
    });
}

/**
 * Handles responsive layout adjustments based on screen size
 */
function handleResponsiveLayout() {
    if (window.vatFilingApp) {
        window.vatFilingApp.handleWindowResize();
    }
}

/**
 * Handles scroll-based UI behaviors like sticky headers
 */
function handleScrollBehavior() {
    if (window.vatFilingApp) {
        window.vatFilingApp.handleScroll();
    }
}

/**
 * Loads application settings from configuration
 * @returns {Promise<object>} Promise resolving to the application settings object
 */
async function loadAppSettings() {
    try {
        const response = await fetch('/appsettings.json');
        if (!response.ok) {
            throw new Error(`Failed to load application settings: ${response.status} ${response.statusText}`);
        }
        
        const settings = await response.json();
        window.appSettings = settings;
        
        return settings;
    } catch (error) {
        console.error('Error loading application settings:', error);
        // Return default settings on error
        return {
            theme: 'light',
            apiBaseUrl: '/api',
            features: {
                darkMode: true,
                offlineMode: true,
                charts: true
            }
        };
    }
}

/**
 * Scrolls the page to the top with smooth animation
 */
function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

/**
 * Creates a debounced version of a function to limit execution frequency
 * @param {Function} func - The function to debounce
 * @param {number} wait - The debounce wait time in milliseconds
 * @param {boolean} immediate - Whether to execute the function immediately
 * @returns {Function} - Debounced function
 */
function debounce(func, wait, immediate) {
    let timeout;
    
    return function executedFunction() {
        const context = this;
        const args = arguments;
        
        const later = function() {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        
        const callNow = immediate && !timeout;
        
        clearTimeout(timeout);
        
        timeout = setTimeout(later, wait);
        
        if (callNow) func.apply(context, args);
    };
}

/**
 * Creates a throttled version of a function to limit execution frequency
 * @param {Function} func - The function to throttle
 * @param {number} limit - The time limit in milliseconds
 * @returns {Function} - Throttled function
 */
function throttle(func, limit) {
    let lastCall = 0;
    
    return function() {
        const now = Date.now();
        if (now - lastCall >= limit) {
            lastCall = now;
            func.apply(this, arguments);
        }
    };
}

/**
 * Stores a value in the browser's local storage or uses window.setLocalStorageItem if available
 * @param {string} key - The storage key
 * @param {string} value - The value to store
 */
function setLocalStorageItem(key, value) {
    try {
        // Use Blazor interop method if available
        if (typeof window.setLocalStorageItem === 'function') {
            window.setLocalStorageItem(key, value);
        } else {
            localStorage.setItem(key, value);
        }
    } catch (error) {
        console.error('Error storing data in local storage:', error);
    }
}

/**
 * Retrieves a value from the browser's local storage or uses window.getLocalStorageItem if available
 * @param {string} key - The storage key
 * @returns {string} - The stored value or null if not found
 */
function getLocalStorageItem(key) {
    try {
        // Use Blazor interop method if available
        if (typeof window.getLocalStorageItem === 'function') {
            return window.getLocalStorageItem(key);
        } else {
            return localStorage.getItem(key);
        }
    } catch (error) {
        console.error('Error retrieving data from local storage:', error);
        return null;
    }
}

// Initialize the application when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', initializeApp);

// Set up main window event listeners with throttling
window.addEventListener('resize', throttle(handleResponsiveLayout, 250));
window.addEventListener('scroll', throttle(handleScrollBehavior, 100));

// Export functions for external use
window.vatFilingApp = window.vatFilingApp || {};
window.scrollToTop = scrollToTop;
window.setLocalStorageItem = setLocalStorageItem;
window.getLocalStorageItem = getLocalStorageItem;