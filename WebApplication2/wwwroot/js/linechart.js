let chart = null;
let currentRange = 'months';

function buildDataset(labels, values) {
    return {
        labels: labels,
        datasets: [{
            label: 'User Registration Trends',
            data: values,
            fill: false,
            borderColor: 'rgb(75, 126, 192)',
            backgroundColor: 'rgb(75, 126, 192)',
            tension: 0.1
        }]
    };
}

function initChart() {
    const ctx = document.getElementById('lineChart');
    if (!ctx) return;

    const initial = window.dashboardInitialData || { labels: [], values: [] };

    chart = new Chart(ctx, {
        type: 'line',
        data: buildDataset(initial.labels, initial.values),
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}

function updateChart(labels, values) {
    if (chart) {
        chart.data = buildDataset(labels, values);
        chart.update();
    }
}

async function refreshDashboard() {
    try {
        const response = await fetch(`/Home/DashboardData?range=${currentRange}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!response.ok) return;
        const data = await response.json();

        const totalEl = document.getElementById('stat-total-users');
        const newRegEl = document.getElementById('stat-new-regs');
        const activeEl = document.getElementById('stat-active-users');
        if (totalEl) totalEl.textContent = data.totalUsers;
        if (newRegEl) newRegEl.textContent = data.newRegistrations;
        if (activeEl) activeEl.textContent = data.activeUsers;

        updateChart(data.labels, data.values);
    } catch (e) {
        console.error('Dashboard refresh failed', e);
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initChart);
} else {
    initChart();
}

const radioButtons = document.querySelectorAll('input[name="radio"]');
radioButtons.forEach(radio => {
    radio.addEventListener('change', function () {
        const label = this.parentElement.textContent.trim();
        currentRange = label.includes('30Days') ? 'days' : 'months';
        refreshDashboard();
    });
});


setInterval(refreshDashboard, 10000);
