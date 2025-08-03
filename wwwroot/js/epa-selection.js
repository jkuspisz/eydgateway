/**
 * EPA Selection Component
 * Handles validation and UI logic for EPA checkbox selection
 * Enforces 1-2 EPA selection limit
 */

class EPASelection {
    constructor(containerId = 'epaSelection') {
        this.container = document.getElementById(containerId);
        this.checkboxes = this.container?.querySelectorAll('.epa-checkbox') || [];
        this.countDisplay = document.getElementById('epaSelectionCount');
        this.errorDisplay = document.getElementById('epaValidationError');
        this.minSelection = 1;
        this.maxSelection = 2;
        
        this.init();
    }
    
    init() {
        if (!this.container) {
            console.warn('EPA Selection container not found');
            return;
        }
        
        // Add event listeners to all EPA checkboxes
        this.checkboxes.forEach(checkbox => {
            checkbox.addEventListener('change', () => this.handleSelectionChange());
        });
        
        // Initialize display
        this.updateDisplay();
    }
    
    handleSelectionChange() {
        const selectedCount = this.getSelectedCount();
        
        // Enforce maximum selection limit
        if (selectedCount > this.maxSelection) {
            this.enforceMaxSelection();
            return;
        }
        
        this.updateDisplay();
        this.validateSelection();
    }
    
    enforceMaxSelection() {
        // Find the last checked checkbox and uncheck it
        const checkedBoxes = Array.from(this.checkboxes).filter(cb => cb.checked);
        if (checkedBoxes.length > this.maxSelection) {
            checkedBoxes[checkedBoxes.length - 1].checked = false;
        }
        
        this.updateDisplay();
        this.showMaxSelectionWarning();
    }
    
    getSelectedCount() {
        return Array.from(this.checkboxes).filter(cb => cb.checked).length;
    }
    
    getSelectedEPAIds() {
        return Array.from(this.checkboxes)
            .filter(cb => cb.checked)
            .map(cb => parseInt(cb.value));
    }
    
    updateDisplay() {
        const count = this.getSelectedCount();
        
        if (this.countDisplay) {
            this.countDisplay.textContent = `${count} EPA${count !== 1 ? 's' : ''} selected`;
            
            // Update count display styling
            this.countDisplay.className = 'text-muted';
            if (count >= this.minSelection && count <= this.maxSelection) {
                this.countDisplay.className = 'text-success';
            } else if (count > this.maxSelection) {
                this.countDisplay.className = 'text-danger';
            }
        }
    }
    
    validateSelection() {
        const count = this.getSelectedCount();
        const isValid = count >= this.minSelection && count <= this.maxSelection;
        
        // Update container styling
        if (this.container) {
            this.container.classList.remove('epa-validation-error', 'epa-validation-success');
            
            if (count > 0) {
                this.container.classList.add(isValid ? 'epa-validation-success' : 'epa-validation-error');
            }
        }
        
        // Show/hide error message
        if (this.errorDisplay) {
            if (isValid || count === 0) {
                this.errorDisplay.classList.add('d-none');
            } else {
                this.errorDisplay.classList.remove('d-none');
            }
        }
        
        return isValid;
    }
    
    showMaxSelectionWarning() {
        // Create temporary warning message
        const warningDiv = document.createElement('div');
        warningDiv.className = 'alert alert-warning alert-dismissible fade show mt-2';
        warningDiv.innerHTML = `
            <small><strong>Maximum Selection:</strong> You can only select up to ${this.maxSelection} EPAs.</small>
            <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="alert"></button>
        `;
        
        this.container.appendChild(warningDiv);
        
        // Auto-remove warning after 3 seconds
        setTimeout(() => {
            if (warningDiv && warningDiv.parentNode) {
                warningDiv.remove();
            }
        }, 3000);
    }
    
    // Public method to validate before form submission
    isValidForSubmission() {
        const count = this.getSelectedCount();
        const isValid = count >= this.minSelection && count <= this.maxSelection;
        
        if (!isValid) {
            this.validateSelection(); // Show error state
            
            // Scroll to EPA selection if invalid
            this.container.scrollIntoView({ 
                behavior: 'smooth', 
                block: 'center' 
            });
        }
        
        return isValid;
    }
    
    // Public method to reset selection
    reset() {
        this.checkboxes.forEach(checkbox => {
            checkbox.checked = false;
        });
        this.updateDisplay();
        this.validateSelection();
    }
    
    // Public method to set selection programmatically
    setSelection(epaIds) {
        this.reset();
        
        if (!Array.isArray(epaIds) || epaIds.length > this.maxSelection) {
            console.warn('Invalid EPA selection provided');
            return;
        }
        
        epaIds.forEach(epaId => {
            const checkbox = this.container.querySelector(`input[value="${epaId}"]`);
            if (checkbox) {
                checkbox.checked = true;
            }
        });
        
        this.updateDisplay();
        this.validateSelection();
    }
}

// Form submission validation helper
function validateEPASelectionOnSubmit(event, epaSelectionInstance) {
    if (!epaSelectionInstance.isValidForSubmission()) {
        event.preventDefault();
        return false;
    }
    return true;
}

// Auto-initialize EPA selection when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Check if EPA selection container exists
    if (document.getElementById('epaSelection')) {
        window.epaSelection = new EPASelection();
        
        // Add form submission validation if form exists
        const form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', function(event) {
                validateEPASelectionOnSubmit(event, window.epaSelection);
            });
        }
    }
});
