class AuthProgressController {
    constructor() {
        this.progressElement = document.getElementById('auth-progress');
        this.steps = document.querySelectorAll('.auth-step');
        this.messageElement = document.getElementById('progress-message');
        this.currentStep = null;
    }

    show() {
        if (this.progressElement) {
            this.progressElement.style.display = 'block';
            this.reset();
            // Scroll to progress if needed
            this.progressElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }

    hide() {
        setTimeout(() => {
            if (this.progressElement) {
                this.progressElement.style.display = 'none';
            }
        }, 500);
    }

    setStep(stepName, message = null) {
        // Remove active and completed from all steps
        this.steps.forEach(step => {
            step.classList.remove('active', 'completed');
        });

        // Find and activate current step
        const currentStepElement = document.querySelector(`[data-step="${stepName}"]`);
        if (currentStepElement) {
            currentStepElement.classList.add('active');
            this.currentStep = stepName;

            // Mark previous steps as completed
            const allSteps = Array.from(this.steps);
            const currentIndex = allSteps.findIndex(s => s.dataset.step === stepName);
            
            allSteps.forEach((step, index) => {
                if (index < currentIndex) {
                    step.classList.add('completed');
                }
            });
        }

        // Update message if provided
        if (message && this.messageElement) {
            this.messageElement.textContent = message;
        }
    }

    updateMessage(message) {
        if (this.messageElement) {
            this.messageElement.textContent = message;
        }
    }

    reset() {
        this.steps.forEach(step => {
            step.classList.remove('active', 'completed');
        });
        if (this.messageElement) {
            this.messageElement.textContent = 'Setting up your personalized workspace...';
        }
    }

    // Simulate progress for each step
    async simulateProgress() {
        const steps = ['validate', 'claims', 'session', 'complete'];
        const messages = [
            'Validating your credentials...',
            'Building your personalized permissions...',
            'Creating secure session...',
            'Almost done, preparing your workspace...'
        ];

        for (let i = 0; i < steps.length; i++) {
            this.setStep(steps[i], messages[i]);
            await new Promise(resolve => setTimeout(resolve, 800));
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.authProgress = new AuthProgressController();
});
