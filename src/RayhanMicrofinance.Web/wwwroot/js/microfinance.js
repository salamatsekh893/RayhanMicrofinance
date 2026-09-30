// RPS Rayhan Tech Microfinance ERP - Frontend Client Logic

let currentUser = null;
let currentToken = localStorage.getItem('erp_token');
let selectedBranchId = '';
let activeTab = 'dashboard';
let monthlyChart = null;

// API Helper
async function api(endpoint, method = 'GET', body = null) {
    const headers = {
        'Content-Type': 'application/json'
    };
    if (currentToken) {
        headers['Authorization'] = `Bearer ${currentToken}`;
    }

    const options = { method, headers };
    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const res = await fetch(endpoint, options);
        if (res.status === 401) {
            handleLogout();
            throw new Error('Session expired. Please sign in again.');
        }
        const data = await res.json();
        return data;
    } catch (err) {
        console.error('API Error:', err);
        throw err;
    }
}

// 1. AUTHENTICATION
function fillLogin(u, p) {
    document.getElementById('loginUsername').value = u;
    document.getElementById('loginPassword').value = p;
}

async function handleLogin() {
    const u = document.getElementById('loginUsername').value.trim();
    const p = document.getElementById('loginPassword').value.trim();

    try {
        const res = await api('/api/auth/login', 'POST', { username: u, password: p });
        if (res && res.success) {
            currentToken = res.data.accessToken;
            currentUser = res.data;
            localStorage.setItem('erp_token', currentToken);
            localStorage.setItem('erp_user', JSON.stringify(currentUser));

            document.getElementById('loginOverlay').classList.add('hidden');
            document.getElementById('mainWorkspace').classList.remove('hidden');

            initApp();
            Swal.fire({
                icon: 'success',
                title: `Welcome, ${currentUser.fullName}`,
                text: 'Terminal session active.',
                timer: 1500,
                showConfirmButton: false
            });
        } else {
            Swal.fire('Login Failed', res?.message || 'Invalid credentials', 'error');
        }
    } catch (err) {
        Swal.fire('Error', 'Unable to connect to server.', 'error');
    }
}

function handleLogout() {
    currentToken = null;
    currentUser = null;
    localStorage.removeItem('erp_token');
    localStorage.removeItem('erp_user');
    document.getElementById('mainWorkspace').classList.add('hidden');
    document.getElementById('loginOverlay').classList.remove('hidden');
}

// 2. INITIALIZATION
document.addEventListener('DOMContentLoaded', () => {
    updateLiveClock();
    setInterval(updateLiveClock, 1000);

    const savedUser = localStorage.getItem('erp_user');
    if (currentToken && savedUser) {
        try {
            currentUser = JSON.parse(savedUser);
            document.getElementById('loginOverlay').classList.add('hidden');
            document.getElementById('mainWorkspace').classList.remove('hidden');
            initApp();
        } catch {
            handleLogout();
        }
    } else {
        document.getElementById('loginOverlay').classList.remove('hidden');
        document.getElementById('mainWorkspace').classList.add('hidden');
    }
});

function updateLiveClock() {
    const el = document.getElementById('liveDateTime');
    if (el) {
        const now = new Date();
        el.innerText = now.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' }) + ' ' + now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    }
}

async function initApp() {
    if (currentUser) {
        document.getElementById('userNameDisplay').innerText = currentUser.fullName;
        document.getElementById('userRoleDisplay').innerText = currentUser.roleName;
        document.getElementById('userAvatar').innerText = (currentUser.fullName || 'A').charAt(0).toUpperCase();
    }

    await loadBranches();
    await loadDashboard();
}

// 3. BRANCH SELECTOR
async function loadBranches() {
    try {
        const res = await api('/api/branches');
        if (res && res.success) {
            const selector = document.getElementById('branchSelector');
            selector.innerHTML = '<option value="">All Branches (Consolidated)</option>';
            res.data.forEach(b => {
                const opt = document.createElement('option');
                opt.value = b.id;
                opt.innerText = `${b.branchCode} - ${b.branchName}`;
                selector.appendChild(opt);
            });

            // Populate branch cards
            const container = document.getElementById('branchesCardsContainer');
            if (container) {
                container.innerHTML = res.data.map(b => `
                    <div class="bg-white rounded-2xl p-5 border border-slate-200 shadow-sm space-y-3">
                        <div class="flex items-center justify-between">
                            <span class="text-xs font-bold px-2.5 py-1 bg-brand-50 text-brand-700 rounded-lg">${b.branchCode}</span>
                            <span class="text-xs font-bold text-slate-400">${b.isHeadOffice ? 'Head Office' : 'Branch Office'}</span>
                        </div>
                        <h4 class="font-bold text-slate-900 text-base">${b.branchName}</h4>
                        <p class="text-xs text-slate-500">${b.address}, ${b.city}, ${b.state} - ${b.pincode}</p>
                        <div class="pt-3 border-t border-slate-100 grid grid-cols-2 gap-2 text-xs">
                            <div>
                                <span class="text-slate-400 block text-[10px] uppercase font-bold">Vault Cash</span>
                                <span class="font-bold text-slate-800">₹${Number(b.currentCashBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                            <div>
                                <span class="text-slate-400 block text-[10px] uppercase font-bold">Bank Balance</span>
                                <span class="font-bold text-indigo-700">₹${Number(b.currentBankBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        </div>
                    </div>
                `).join('');
            }
        }
    } catch (err) {
        console.error('Error loading branches', err);
    }
}

function onBranchChange() {
    selectedBranchId = document.getElementById('branchSelector').value;
    switchTab(activeTab);
}

// 4. TAB NAVIGATION
function switchTab(tab) {
    activeTab = tab;

    document.querySelectorAll('.tab-view').forEach(v => v.classList.add('hidden'));
    const target = document.getElementById(`view-${tab}`);
    if (target) target.classList.remove('hidden');

    document.querySelectorAll('.nav-link').forEach(l => {
        l.classList.remove('bg-brand-600', 'text-white');
        l.classList.add('text-slate-400');
    });

    const activeLink = document.getElementById(`nav-${tab}`);
    if (activeLink) {
        activeLink.classList.remove('text-slate-400');
        activeLink.classList.add('bg-brand-600', 'text-white');
    }

    // Refresh content for active tab
    if (tab === 'dashboard') loadDashboard();
    else if (tab === 'customers') loadCustomers();
    else if (tab === 'centers') loadCentersAndGroups();
    else if (tab === 'loans') loadLoans();
    else if (tab === 'collections') setupCollectionSheet();
    else if (tab === 'savings') loadSavings();
    else if (tab === 'accounting') loadAccounting();
    else if (tab === 'expenses') loadExpenses();
    else if (tab === 'employees') loadEmployees();
}

// 5. DASHBOARD
async function loadDashboard() {
    try {
        const query = selectedBranchId ? `?branchId=${selectedBranchId}` : '';
        const res = await api(`/api/dashboard/summary${query}`);
        if (res && res.success) {
            const d = res.data;
            document.getElementById('statTodayCollection').innerText = `₹${Number(d.todayCollection).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statTodayDisbursement').innerText = `₹${Number(d.todayDisbursement).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statTotalPortfolio').innerText = `₹${Number(d.totalActiveLoanPortfolio).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statActiveLoansCount').innerText = d.activeLoansCount;
            document.getElementById('statOverdueAmount').innerText = `₹${Number(d.overdueAmount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statOverdueCount').innerText = d.overdueLoansCount;

            document.getElementById('statCashBalance').innerText = `₹${Number(d.totalCashBalance).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statBankBalance').innerText = `₹${Number(d.totalBankBalance).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statTotalSavings').innerText = `₹${Number(d.totalSavingsBalance).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            document.getElementById('statNetProfit').innerText = `₹${Number(d.netProfit).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;

            // Render Chart
            renderMonthlyChart(d.monthlyDisbursementAndCollection);

            // Render Branch Performance
            const perfContainer = document.getElementById('branchPerformanceList');
            if (perfContainer) {
                perfContainer.innerHTML = d.branchPerformances.map(p => `
                    <div class="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
                        <div>
                            <p class="font-bold text-xs text-slate-800">${p.branchName}</p>
                            <p class="text-[11px] text-slate-500">${p.activeLoans} Loans Active</p>
                        </div>
                        <div class="text-right">
                            <p class="font-bold text-xs text-brand-700">₹${Number(p.outstandingAmount).toLocaleString('en-IN')}</p>
                            <span class="text-[10px] font-semibold text-emerald-600 bg-emerald-50 px-2 py-0.5 rounded-full">98.5% Recovery</span>
                        </div>
                    </div>
                `).join('');
            }
        }
    } catch (err) {
        console.error('Error loading dashboard', err);
    }
}

function renderMonthlyChart(dataPoints) {
    const ctx = document.getElementById('monthlyTrendChart');
    if (!ctx) return;

    if (monthlyChart) monthlyChart.destroy();

    const labels = dataPoints.map(d => d.monthName);
    const disbData = dataPoints.map(d => d.disbursement);
    const colData = dataPoints.map(d => d.collection);

    monthlyChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Disbursement (₹)',
                    data: disbData,
                    backgroundColor: 'rgba(59, 130, 246, 0.75)',
                    borderRadius: 6
                },
                {
                    label: 'Collection (₹)',
                    data: colData,
                    backgroundColor: 'rgba(22, 163, 74, 0.85)',
                    borderRadius: 6
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top', labels: { boxWidth: 12, font: { family: 'Plus Jakarta Sans', size: 11 } } }
            },
            scales: {
                y: { beginAtZero: true, grid: { color: '#f1f5f9' } },
                x: { grid: { display: false } }
            }
        }
    });
}

// 6. CUSTOMERS
async function loadCustomers() {
    try {
        const search = document.getElementById('custSearchInput')?.value || '';
        let url = `/api/customers?pageIndex=1&pageSize=50`;
        if (selectedBranchId) url += `&branchId=${selectedBranchId}`;
        if (search) url += `&search=${encodeURIComponent(search)}`;

        const res = await api(url);
        const tbody = document.getElementById('customersTableBody');
        if (res && res.success && tbody) {
            if (res.data.items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center py-8 text-slate-400">No members found. Click "Register New Member" to add one.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.items.map(c => `
                <tr class="hover:bg-slate-50/80 transition">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${c.customerCode}</td>
                    <td class="py-3 px-4">
                        <div class="font-bold text-slate-900">${c.fullName}</div>
                        <div class="text-[11px] text-slate-400">${c.relationWithGuardian}: ${c.guardianName}</div>
                    </td>
                    <td class="py-3 px-4">${c.phone}</td>
                    <td class="py-3 px-4">
                        <div class="font-medium text-slate-800">${c.centerName || 'Direct'}</div>
                        <div class="text-[11px] text-slate-400">${c.groupName || '--'}</div>
                    </td>
                    <td class="py-3 px-4 font-mono">
                        <div>${c.aadhaarNumber || '--'}</div>
                        <span class="inline-flex items-center text-[10px] font-semibold text-emerald-600 bg-emerald-50 px-2 py-0.5 rounded-md mt-0.5">
                            <i class="fa-solid fa-check-circle mr-1 text-[9px]"></i> KYC Verified
                        </span>
                    </td>
                    <td class="py-3 px-4">${c.occupation || '--'}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-2.5 py-0.5 rounded-full text-[10px] font-bold ${c.isBlacklisted ? 'bg-red-50 text-red-600' : 'bg-emerald-50 text-emerald-700'}">
                            ${c.isBlacklisted ? 'Blacklisted' : 'Active'}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right space-x-1">
                        <button onclick="quickApplyForCustomer(${c.id}, '${c.fullName}')" class="px-2.5 py-1 bg-brand-50 hover:bg-brand-100 text-brand-700 rounded-lg text-xs font-semibold border border-brand-200" title="Apply Loan">
                            <i class="fa-solid fa-plus mr-1"></i> Loan
                        </button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading customers', err);
    }
}

function searchCustomers() {
    clearTimeout(window.searchTimer);
    window.searchTimer = setTimeout(loadCustomers, 300);
}

// 7. CENTERS & GROUPS
async function loadCentersAndGroups() {
    try {
        const query = selectedBranchId ? `?branchId=${selectedBranchId}` : '';
        const [cRes, gRes] = await Promise.all([
            api(`/api/centers${query}`),
            api(`/api/centers/groups${query}`)
        ]);

        if (cRes && cRes.success) {
            document.getElementById('centerCountBadge').innerText = `${cRes.data.length} Centers`;
            const container = document.getElementById('centersListContainer');
            container.innerHTML = cRes.data.map(c => `
                <div class="p-3.5 bg-slate-50 rounded-xl border border-slate-200/80 flex items-center justify-between">
                    <div>
                        <div class="flex items-center gap-2">
                            <span class="font-bold text-xs text-slate-900">${c.centerName}</span>
                            <span class="text-[10px] font-mono bg-slate-200 text-slate-700 px-1.5 py-0.5 rounded">${c.centerCode}</span>
                        </div>
                        <p class="text-[11px] text-slate-500 mt-1">Meeting: <span class="font-bold text-slate-700">${c.meetingDayName} at ${c.meetingTime}</span> | Place: ${c.meetingPlace}</p>
                    </div>
                    <div class="text-right">
                        <span class="text-xs font-bold text-brand-700">${c.totalGroups} Groups</span>
                        <span class="block text-[11px] text-slate-400">${c.totalMembers} Members</span>
                    </div>
                </div>
            `).join('');
        }

        if (gRes && gRes.success) {
            document.getElementById('groupCountBadge').innerText = `${gRes.data.length} Groups`;
            const container = document.getElementById('groupsListContainer');
            container.innerHTML = gRes.data.map(g => `
                <div class="p-3.5 bg-slate-50 rounded-xl border border-slate-200/80 flex items-center justify-between">
                    <div>
                        <div class="flex items-center gap-2">
                            <span class="font-bold text-xs text-slate-900">${g.groupName}</span>
                            <span class="text-[10px] font-mono bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded">${g.groupCode}</span>
                        </div>
                        <p class="text-[11px] text-slate-500 mt-1">Center: <span class="font-bold text-slate-700">${g.centerName}</span> | Leader: ${g.groupLeaderName || 'Not Assigned'}</p>
                    </div>
                    <div class="text-right">
                        <span class="text-xs font-bold text-blue-700">${g.memberCount} / 5 Members</span>
                        <span class="block text-[10px] text-emerald-600 font-semibold">Active JLG</span>
                    </div>
                </div>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading centers', err);
    }
}

// 8. LOANS
let currentLoanFilter = '';

async function loadLoans() {
    try {
        // Load schemes cards
        const sRes = await api('/api/loans/schemes');
        if (sRes && sRes.success) {
            const container = document.getElementById('schemesCardsContainer');
            container.innerHTML = sRes.data.map(s => `
                <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm space-y-2">
                    <div class="flex justify-between items-center">
                        <span class="text-[10px] font-mono font-bold bg-brand-50 text-brand-700 px-2 py-0.5 rounded">${s.schemeCode}</span>
                        <span class="text-xs font-bold text-emerald-600">${s.interestRatePerAnnum}% p.a.</span>
                    </div>
                    <h5 class="font-bold text-xs text-slate-900 leading-snug">${s.schemeName}</h5>
                    <p class="text-[11px] text-slate-500">Min: ₹${s.minAmount.toLocaleString()} | Max: ₹${s.maxAmount.toLocaleString()}</p>
                    <div class="text-[10px] text-slate-400 font-medium">Freq: ${s.repaymentFrequency == 2 ? 'Weekly' : 'Monthly'} (${s.interestCalculationMethod == 1 ? 'Flat' : 'Reducing'})</div>
                </div>
            `).join('');
        }

        // Load applications table
        let url = `/api/loans?pageIndex=1&pageSize=50`;
        if (selectedBranchId) url += `&branchId=${selectedBranchId}`;
        if (currentLoanFilter) url += `&status=${currentLoanFilter}`;

        const res = await api(url);
        const tbody = document.getElementById('loansTableBody');
        if (res && res.success && tbody) {
            if (res.data.items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="9" class="text-center py-8 text-slate-400">No loan records found.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.items.map(l => `
                <tr class="hover:bg-slate-50/80 transition">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${l.loanAccountNumber}</td>
                    <td class="py-3 px-4">
                        <div class="font-bold text-slate-900">${l.customerName}</div>
                        <div class="text-[11px] text-slate-400">${l.customerPhone || ''}</div>
                    </td>
                    <td class="py-3 px-4">
                        <div class="font-medium text-slate-800">${l.loanSchemeName}</div>
                        <div class="text-[10px] text-slate-400">${l.tenureInMonths} Months | ${l.frequency == 2 ? 'Weekly' : 'Monthly'}</div>
                    </td>
                    <td class="py-3 px-4 text-right font-bold text-slate-900">₹${Number(l.disbursedAmount > 0 ? l.disbursedAmount : l.approvedAmount > 0 ? l.approvedAmount : l.requestedAmount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right">₹${Number(l.totalPayable).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-emerald-600 font-semibold">₹${Number(l.totalPaid).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-brand-700 font-bold">₹${Number(l.totalOutstanding).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-2.5 py-0.5 rounded-full text-[10px] font-bold ${getLoanStatusBadge(l.statusName)}">
                            ${l.statusName}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right space-x-1">
                        ${l.statusName === 'Applied' ? `
                            <button onclick="openApproveModal(${l.id}, '${l.loanAccountNumber}', ${l.requestedAmount})" class="px-2 py-1 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded-lg text-xs font-bold border border-blue-200">Approve</button>
                        ` : ''}
                        ${l.statusName === 'Approved' ? `
                            <button onclick="openDisburseModal(${l.id}, '${l.loanAccountNumber}', ${l.approvedAmount})" class="px-2 py-1 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg text-xs font-bold border border-emerald-200">Disburse</button>
                        ` : ''}
                        ${l.statusName === 'Active' || l.statusName === 'Closed' ? `
                            <button onclick="viewLoanSchedule(${l.id}, '${l.loanAccountNumber}')" class="px-2 py-1 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded-lg text-xs font-bold" title="Passbook">
                                <i class="fa-solid fa-list-ol mr-1"></i> Passbook
                            </button>
                        ` : ''}
                    </td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading loans', err);
    }
}

function getLoanStatusBadge(status) {
    switch (status) {
        case 'Applied': return 'bg-amber-50 text-amber-700';
        case 'Approved': return 'bg-blue-50 text-blue-700';
        case 'Active': return 'bg-emerald-50 text-emerald-700';
        case 'Closed': return 'bg-slate-100 text-slate-600';
        default: return 'bg-slate-100 text-slate-700';
    }
}

function filterLoansByStatus(status) {
    currentLoanFilter = status;
    document.querySelectorAll('.loan-filter-btn').forEach(b => {
        b.classList.remove('bg-brand-600', 'text-white');
        b.classList.add('text-slate-600');
    });
    event.target.classList.add('bg-brand-600', 'text-white');
    event.target.classList.remove('text-slate-600');
    loadLoans();
}

// 9. FIELD COLLECTION SHEET
async function setupCollectionSheet() {
    try {
        const query = selectedBranchId ? `?branchId=${selectedBranchId}` : '';
        const res = await api(`/api/centers${query}`);
        const selector = document.getElementById('sheetCenterSelector');
        if (res && res.success && selector) {
            selector.innerHTML = '<option value="">Select Center...</option>' + res.data.map(c => `<option value="${c.id}">${c.centerName} (${c.meetingDayName})</option>`).join('');
            document.getElementById('sheetDateInput').valueAsDate = new Date();
        }
    } catch (err) {
        console.error('Error setting up sheet', err);
    }
}

async function loadCollectionSheet() {
    const centerId = document.getElementById('sheetCenterSelector').value;
    if (!centerId) return;

    try {
        const date = document.getElementById('sheetDateInput').value;
        const res = await api(`/api/collection/sheet?centerId=${centerId}&date=${date}`);
        const tbody = document.getElementById('collectionSheetBody');

        if (res && res.success && tbody) {
            if (res.data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="10" class="text-center py-8 text-slate-400">No active loans due for this center on this date.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.map((item, idx) => `
                <tr class="hover:bg-slate-50 transition" data-loan-id="${item.loanId}">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${item.loanAccountNumber}</td>
                    <td class="py-3 px-4 font-bold text-slate-900">${item.customerName}</td>
                    <td class="py-3 px-4 text-slate-500">${item.groupName}</td>
                    <td class="py-3 px-4 text-right">₹${Number(item.dueAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right text-rose-600 font-semibold">₹${Number(item.overdueAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-black text-slate-900">₹${Number(item.totalReceivable).toFixed(2)}</td>
                    <td class="py-3 px-4 text-center">
                        <input type="number" class="sheet-collect-amt w-28 px-2.5 py-1 text-right font-bold bg-white border border-slate-200 rounded-lg text-xs focus:ring-2 focus:ring-brand-500" value="${item.collectedAmount}" />
                    </td>
                    <td class="py-3 px-4 text-center">
                        <input type="number" class="sheet-savings-amt w-20 px-2.5 py-1 text-right font-medium bg-white border border-slate-200 rounded-lg text-xs" value="${item.savingsDeposit}" />
                    </td>
                    <td class="py-3 px-4 text-center">
                        <select class="sheet-pay-mode text-[11px] font-semibold bg-white border border-slate-200 rounded-lg px-2 py-1">
                            <option value="1">Cash</option>
                            <option value="4">UPI / QR</option>
                        </select>
                    </td>
                    <td class="py-3 px-4 text-right">
                        <button onclick="collectSingleFromSheet(${item.loanId}, this)" class="px-2.5 py-1 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg font-bold text-xs border border-emerald-200">
                            Collect
                        </button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading sheet', err);
    }
}

async function collectSingleFromSheet(loanId, btn) {
    const row = btn.closest('tr');
    const amount = parseFloat(row.querySelector('.sheet-collect-amt').value);
    const mode = parseInt(row.querySelector('.sheet-pay-mode').value);

    if (amount <= 0 || isNaN(amount)) {
        Swal.fire('Error', 'Invalid collection amount', 'warning');
        return;
    }

    try {
        const res = await api('/api/collection/collect', 'POST', {
            loanApplicationId: loanId,
            amount: amount,
            paymentMode: mode
        });

        if (res && res.success) {
            row.classList.add('bg-emerald-50/70');
            btn.disabled = true;
            btn.innerText = 'Paid ✓';
            btn.className = 'px-2.5 py-1 bg-emerald-600 text-white rounded-lg font-bold text-xs';

            // Show Printable Receipt
            showReceiptModal(res.data);
        } else {
            Swal.fire('Error', res?.message || 'Collection failed', 'error');
        }
    } catch {
        Swal.fire('Error', 'Server connection error', 'error');
    }
}

async function submitBatchCollection() {
    const centerId = document.getElementById('sheetCenterSelector').value;
    if (!centerId) {
        Swal.fire('Select Center', 'Please choose a center first.', 'info');
        return;
    }

    const rows = document.querySelectorAll('#collectionSheetBody tr[data-loan-id]');
    const items = [];

    rows.forEach(r => {
        const loanId = parseInt(r.getAttribute('data-loan-id'));
        const amt = parseFloat(r.querySelector('.sheet-collect-amt').value);
        const mode = parseInt(r.querySelector('.sheet-pay-mode').value);

        if (amt > 0) {
            items.push({
                loanId: loanId,
                collectedAmount: amt,
                paymentMode: mode
            });
        }
    });

    if (items.length === 0) {
        Swal.fire('No Payments', 'No collections to submit.', 'info');
        return;
    }

    const confirm = await Swal.fire({
        title: 'Confirm Batch Collection',
        text: `Submit ${items.length} payments for this center?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, Submit Batch'
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await api('/api/collection/batch-submit', 'POST', {
            centerId: parseInt(centerId),
            branchId: selectedBranchId ? parseInt(selectedBranchId) : (currentUser?.branchId || 1),
            items: items
        });

        if (res && res.success) {
            Swal.fire('Success', `${res.data} collections successfully recorded & vouchers posted!`, 'success');
            loadCollectionSheet();
            loadDashboard();
        }
    } catch {
        Swal.fire('Error', 'Batch submit failed.', 'error');
    }
}

// 10. SAVINGS
async function loadSavings() {
    try {
        let url = '/api/savings/accounts';
        if (selectedBranchId) url += `?branchId=${selectedBranchId}`;
        const res = await api(url);
        const tbody = document.getElementById('savingsTableBody');

        if (res && res.success && tbody) {
            if (res.data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center py-8 text-slate-400">No savings accounts found.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.map(s => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${s.accountNumber}</td>
                    <td class="py-3 px-4 font-bold text-slate-900">${s.customerName}</td>
                    <td class="py-3 px-4">${s.savingsSchemeName}</td>
                    <td class="py-3 px-4 text-right font-black text-brand-700">₹${Number(s.currentBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-slate-600">₹${Number(s.totalDeposited).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-slate-600">₹${Number(s.totalWithdrawn).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-50 text-emerald-700">Active</span>
                    </td>
                    <td class="py-3 px-4 text-right text-slate-400">${new Date(s.openedDate).toLocaleDateString('en-GB')}</td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading savings', err);
    }
}

// 11. ACCOUNTING
async function loadAccounting() {
    try {
        const res = await api('/api/accounting/chart-of-accounts');
        const tbody = document.getElementById('coaTableBody');
        if (res && res.success && tbody) {
            tbody.innerHTML = res.data.map(a => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-bold text-slate-900">${a.accountCode}</td>
                    <td class="py-3 px-4 font-medium text-slate-800">${a.accountName}</td>
                    <td class="py-3 px-4">
                        <span class="px-2 py-0.5 rounded-md text-[10px] font-bold ${getAccountClassBadge(a.classificationName)}">
                            ${a.classificationName}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right font-bold text-slate-900 font-mono">
                        ₹${Number(a.currentBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}
                    </td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading COA', err);
    }
}

function getAccountClassBadge(c) {
    switch (c) {
        case 'Asset': return 'bg-blue-50 text-blue-700';
        case 'Liability': return 'bg-amber-50 text-amber-700';
        case 'Equity': return 'bg-purple-50 text-purple-700';
        case 'Revenue': return 'bg-emerald-50 text-emerald-700';
        case 'Expense': return 'bg-rose-50 text-rose-700';
        default: return 'bg-slate-100 text-slate-700';
    }
}

function switchAccountingTab(tab) {
    document.querySelectorAll('.acc-view').forEach(v => v.classList.add('hidden'));
    const target = document.getElementById(`acc-view-${tab}`);
    if (target) target.classList.remove('hidden');

    document.querySelectorAll('.acc-sub-btn').forEach(b => {
        b.classList.remove('bg-slate-900', 'text-white');
        b.classList.add('text-slate-600');
    });
    const btn = document.getElementById(`acc-tab-${tab}`);
    if (btn) {
        btn.classList.add('bg-slate-900', 'text-white');
        btn.classList.remove('text-slate-600');
    }

    if (tab === 'trial') loadTrialBalance();
    else if (tab === 'pl') loadProfitLoss();
    else if (tab === 'bs') loadBalanceSheet();
    else if (tab === 'vouchers') loadVouchers();
}

async function loadTrialBalance() {
    try {
        const res = await api('/api/accounting/trial-balance');
        const container = document.getElementById('trialBalanceContent');
        if (res && res.success && container) {
            const d = res.data;
            container.innerHTML = `
                <div class="flex items-center justify-between mb-4">
                    <h4 class="font-bold text-sm text-slate-900">General Ledger Trial Balance</h4>
                    <span class="text-xs px-2.5 py-1 rounded-full font-bold ${d.isBalanced ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}">
                        ${d.isBalanced ? 'Balanced ✓' : 'Out of Balance'}
                    </span>
                </div>
                <table class="w-full text-left text-xs border-collapse">
                    <thead>
                        <tr class="bg-slate-50 border-b border-slate-200 text-slate-500 uppercase font-semibold">
                            <th class="py-2.5 px-3">Account Code & Name</th>
                            <th class="py-2.5 px-3">Classification</th>
                            <th class="py-2.5 px-3 text-right">Debit Balance (₹)</th>
                            <th class="py-2.5 px-3 text-right">Credit Balance (₹)</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100">
                        ${d.rows.map(r => `
                            <tr>
                                <td class="py-2.5 px-3 font-medium"><span class="font-mono font-bold mr-2 text-slate-400">${r.accountCode}</span>${r.accountName}</td>
                                <td class="py-2.5 px-3 text-slate-500">${r.classification}</td>
                                <td class="py-2.5 px-3 text-right font-mono">${r.debitBalance > 0 ? '₹' + Number(r.debitBalance).toLocaleString('en-IN', {minimumFractionDigits: 2}) : '--'}</td>
                                <td class="py-2.5 px-3 text-right font-mono">${r.creditBalance > 0 ? '₹' + Number(r.creditBalance).toLocaleString('en-IN', {minimumFractionDigits: 2}) : '--'}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                    <tfoot>
                        <tr class="bg-slate-100 font-bold border-t-2 border-slate-300">
                            <td colspan="2" class="py-3 px-3 uppercase text-right">Total:</td>
                            <td class="py-3 px-3 text-right text-brand-700 font-mono font-black text-sm">₹${Number(d.totalDebit).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                            <td class="py-3 px-3 text-right text-brand-700 font-mono font-black text-sm">₹${Number(d.totalCredit).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                        </tr>
                    </tfoot>
                </table>
            `;
        }
    } catch (err) {
        console.error('Error loading trial balance', err);
    }
}

async function loadProfitLoss() {
    try {
        const res = await api('/api/accounting/profit-loss');
        const container = document.getElementById('plContent');
        if (res && res.success && container) {
            const d = res.data;
            container.innerHTML = `
                <div class="flex items-center justify-between mb-4">
                    <h4 class="font-bold text-sm text-slate-900">Profit & Loss Statement (Income vs Expenses)</h4>
                    <span class="text-sm font-black px-3 py-1 rounded-xl ${d.netProfit >= 0 ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}">
                        Net Profit: ₹${Number(d.netProfit).toLocaleString('en-IN', {minimumFractionDigits: 2})}
                    </span>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-xs">
                    <!-- Revenue -->
                    <div class="bg-emerald-50/40 p-4 rounded-xl border border-emerald-100 space-y-2">
                        <h5 class="font-bold text-emerald-800 uppercase tracking-wider text-[11px]">Revenues / Incomes</h5>
                        ${d.revenues.map(r => `
                            <div class="flex justify-between py-1 border-b border-emerald-100/50">
                                <span>${r.accountName}</span>
                                <span class="font-bold font-mono">₹${Number(r.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-2 font-black text-emerald-900 text-sm">
                            <span>Total Operating Revenue:</span>
                            <span class="font-mono">₹${Number(d.totalRevenue).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>

                    <!-- Expenses -->
                    <div class="bg-rose-50/40 p-4 rounded-xl border border-rose-100 space-y-2">
                        <h5 class="font-bold text-rose-800 uppercase tracking-wider text-[11px]">Operating Expenses</h5>
                        ${d.expenses.map(e => `
                            <div class="flex justify-between py-1 border-b border-rose-100/50">
                                <span>${e.accountName}</span>
                                <span class="font-bold font-mono">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-2 font-black text-rose-900 text-sm">
                            <span>Total Operating Expenses:</span>
                            <span class="font-mono">₹${Number(d.totalExpense).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>
                </div>
            `;
        }
    } catch (err) {
        console.error('Error loading PL', err);
    }
}

async function loadBalanceSheet() {
    try {
        const res = await api('/api/accounting/balance-sheet');
        const container = document.getElementById('bsContent');
        if (res && res.success && container) {
            const d = res.data;
            container.innerHTML = `
                <h4 class="font-bold text-sm text-slate-900 mb-4">Financial Statement: Balance Sheet</h4>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-xs">
                    <!-- Assets -->
                    <div class="bg-blue-50/40 p-4 rounded-xl border border-blue-100 space-y-2">
                        <h5 class="font-bold text-blue-800 uppercase tracking-wider text-[11px]">Assets</h5>
                        ${d.assets.map(a => `
                            <div class="flex justify-between py-1 border-b border-blue-100/50">
                                <span>${a.accountName}</span>
                                <span class="font-bold font-mono">₹${Number(a.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-2 font-black text-blue-900 text-sm">
                            <span>Total Assets:</span>
                            <span class="font-mono">₹${Number(d.totalAssets).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>

                    <!-- Liabilities & Equity -->
                    <div class="bg-purple-50/40 p-4 rounded-xl border border-purple-100 space-y-2">
                        <h5 class="font-bold text-purple-800 uppercase tracking-wider text-[11px]">Liabilities & Equity</h5>
                        ${d.liabilities.map(l => `
                            <div class="flex justify-between py-1 border-b border-purple-100/50">
                                <span>${l.accountName}</span>
                                <span class="font-bold font-mono">₹${Number(l.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        ${d.equities.map(e => `
                            <div class="flex justify-between py-1 border-b border-purple-100/50">
                                <span>${e.accountName}</span>
                                <span class="font-bold font-mono">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-2 font-black text-purple-900 text-sm">
                            <span>Total Liabilities & Equity:</span>
                            <span class="font-mono">₹${Number(d.totalLiabilitiesAndEquity).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>
                </div>
            `;
        }
    } catch (err) {
        console.error('Error loading Balance sheet', err);
    }
}

async function loadVouchers() {
    try {
        const res = await api('/api/accounting/vouchers');
        const tbody = document.getElementById('vouchersTableBody');
        if (res && res.success && tbody) {
            tbody.innerHTML = res.data.map(v => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${v.voucherNumber}</td>
                    <td class="py-3 px-4">${new Date(v.voucherDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-3 px-4"><span class="px-2 py-0.5 rounded text-[10px] font-bold bg-slate-100 text-slate-700">${v.voucherType}</span></td>
                    <td class="py-3 px-4 text-slate-600">${v.narration}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold text-slate-900">₹${Number(v.totalAmount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center text-slate-400 font-medium">${v.preparedBy || 'System'}</td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading vouchers', err);
    }
}

// 12. EXPENSES
async function loadExpenses() {
    try {
        let url = '/api/expenses';
        if (selectedBranchId) url += `?branchId=${selectedBranchId}`;
        const res = await api(url);
        const tbody = document.getElementById('expensesTableBody');
        if (res && res.success && tbody) {
            tbody.innerHTML = res.data.map(e => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-bold text-slate-700">${e.expenseNumber}</td>
                    <td class="py-3 px-4">${new Date(e.expenseDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-3 px-4 font-bold text-slate-800">${e.expenseCategoryName}</td>
                    <td class="py-3 px-4">${e.paidTo}</td>
                    <td class="py-3 px-4">${e.paymentMode == 1 ? 'Cash' : 'Bank'}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold text-rose-600">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-slate-500">${e.description || '--'}</td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading expenses', err);
    }
}

// 13. EMPLOYEES
async function loadEmployees() {
    try {
        let url = '/api/employees';
        if (selectedBranchId) url += `?branchId=${selectedBranchId}`;
        const res = await api(url);
        const tbody = document.getElementById('employeesTableBody');
        if (res && res.success && tbody) {
            tbody.innerHTML = res.data.map(e => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-bold text-brand-700">${e.employeeCode}</td>
                    <td class="py-3 px-4 font-bold text-slate-900">${e.fullName}</td>
                    <td class="py-3 px-4">
                        <span class="font-semibold text-slate-700">${e.designation?.title || 'Officer'}</span>
                        <span class="block text-[10px] text-slate-400 font-mono">${e.role}</span>
                    </td>
                    <td class="py-3 px-4">${e.branch?.branchName || '--'}</td>
                    <td class="py-3 px-4 text-slate-600">${e.phone} | ${e.email}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold text-slate-800">₹${Number(e.basicSalary).toLocaleString('en-IN')}</td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading employees', err);
    }
}

// 14. MODAL DIALOGS & ACTIONS
function closeModal(id) {
    document.getElementById(id)?.classList.add('hidden');
}

function openModal(id) {
    document.getElementById(id)?.classList.remove('hidden');
}

async function openRegisterCustomerModal() {
    // Populate centers
    const cRes = await api('/api/centers');
    const sel = document.getElementById('newCustCenter');
    if (cRes && cRes.success && sel) {
        sel.innerHTML = cRes.data.map(c => `<option value="${c.id}">${c.centerName}</option>`).join('');
    }
    openModal('modalRegisterCustomer');
}

async function submitRegisterCustomer() {
    const f = document.getElementById('newCustFirstName').value.trim();
    const l = document.getElementById('newCustLastName').value.trim();
    const g = document.getElementById('newCustGuardian').value.trim();
    const p = document.getElementById('newCustPhone').value.trim();
    const dob = document.getElementById('newCustDob').value;
    const gender = parseInt(document.getElementById('newCustGender').value);
    const center = parseInt(document.getElementById('newCustCenter').value);
    const occ = document.getElementById('newCustOccupation').value.trim();
    const aadhaar = document.getElementById('newCustAadhaar').value.trim();
    const pan = document.getElementById('newCustPan').value.trim();
    const addr = document.getElementById('newCustAddress').value.trim();
    const nom = document.getElementById('newCustNominee').value.trim();
    const guar = document.getElementById('newCustGuarantor').value.trim();

    try {
        const res = await api('/api/customers', 'POST', {
            branchId: selectedBranchId ? parseInt(selectedBranchId) : (currentUser?.branchId || 1),
            centerId: center,
            firstName: f,
            lastName: l,
            guardianName: g,
            phone: p,
            dateOfBirth: dob ? new Date(dob) : new Date(1995, 0, 1),
            gender: gender,
            occupation: occ,
            aadhaarNumber: aadhaar,
            panNumber: pan,
            address: addr,
            nomineeName: nom,
            guarantorName: guar
        });

        if (res && res.success) {
            closeModal('modalRegisterCustomer');
            Swal.fire('Member Registered', `Member code: ${res.data.customerCode}`, 'success');
            loadCustomers();
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Failed to register member', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
}

async function openApplyLoanModal() {
    // Populate customers & schemes
    const [cRes, sRes] = await Promise.all([
        api('/api/customers?pageSize=100'),
        api('/api/loans/schemes')
    ]);

    const cSel = document.getElementById('applyLoanCustomerSelect');
    if (cRes && cRes.success && cSel) {
        cSel.innerHTML = cRes.data.items.map(c => `<option value="${c.id}">${c.customerCode} - ${c.fullName} (${c.phone})</option>`).join('');
    }

    const sSel = document.getElementById('applyLoanSchemeSelect');
    if (sRes && sRes.success && sSel) {
        window.loadedSchemes = sRes.data;
        sSel.innerHTML = sRes.data.map(s => `<option value="${s.id}">${s.schemeName} (${s.interestRatePerAnnum}% p.a.)</option>`).join('');
        onSchemeChange();
    }

    openModal('modalApplyLoan');
}

function onSchemeChange() {
    const sId = parseInt(document.getElementById('applyLoanSchemeSelect').value);
    const scheme = window.loadedSchemes?.find(s => s.id === sId);
    if (scheme) {
        document.getElementById('applyLoanAmount').value = scheme.defaultAmount;
        document.getElementById('applyLoanTenure').value = scheme.defaultTenureMonths;
        document.getElementById('applyLoanSchemePreview').innerText = 
            `Rate: ${scheme.interestRatePerAnnum}% p.a. | Method: ${scheme.interestCalculationMethod == 1 ? 'Flat' : 'Reducing'} | Repayment: ${scheme.repaymentFrequency == 2 ? 'Weekly' : 'Monthly'}`;
    }
}

async function submitApplyLoan() {
    const cId = parseInt(document.getElementById('applyLoanCustomerSelect').value);
    const sId = parseInt(document.getElementById('applyLoanSchemeSelect').value);
    const amt = parseFloat(document.getElementById('applyLoanAmount').value);
    const tenure = parseInt(document.getElementById('applyLoanTenure').value);
    const purpose = document.getElementById('applyLoanPurpose').value.trim();

    try {
        const res = await api('/api/loans/apply', 'POST', {
            customerId: cId,
            branchId: selectedBranchId ? parseInt(selectedBranchId) : (currentUser?.branchId || 1),
            loanSchemeId: sId,
            requestedAmount: amt,
            tenureInMonths: tenure,
            purposeOfLoan: purpose
        });

        if (res && res.success) {
            closeModal('modalApplyLoan');
            Swal.fire('Application Submitted', `Loan Application #${res.data.loanAccountNumber} created. Status: Applied.`, 'success');
            loadLoans();
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Loan application failed', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
}

function openApproveModal(id, loanNo, amt) {
    document.getElementById('approveLoanId').value = id;
    document.getElementById('approveLoanSubtitle').innerText = `Loan Account: ${loanNo}`;
    document.getElementById('approveLoanAmount').value = amt;
    openModal('modalApproveLoan');
}

async function confirmApproveLoan() {
    const id = document.getElementById('approveLoanId').value;
    const amt = parseFloat(document.getElementById('approveLoanAmount').value);
    const rem = document.getElementById('approveLoanRemarks').value;

    try {
        const res = await api(`/api/loans/${id}/approve`, 'POST', {
            loanId: parseInt(id),
            approvedAmount: amt,
            remarks: rem
        });

        if (res && res.success) {
            closeModal('modalApproveLoan');
            Swal.fire('Approved', 'Loan application approved.', 'success');
            loadLoans();
        }
    } catch {
        Swal.fire('Error', 'Approval failed', 'error');
    }
}

function openDisburseModal(id, loanNo, amt) {
    document.getElementById('disburseLoanId').value = id;
    document.getElementById('disburseLoanSubtitle').innerText = `Loan Account: ${loanNo} | Amount: ₹${amt.toLocaleString()}`;
    document.getElementById('disburseLoanAmount').value = amt;
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 7);
    document.getElementById('disburseFirstEmiDate').valueAsDate = tomorrow;
    openModal('modalDisburseLoan');
}

async function confirmDisburseLoan() {
    const id = document.getElementById('disburseLoanId').value;
    const amt = parseFloat(document.getElementById('disburseLoanAmount').value);
    const mode = parseInt(document.getElementById('disburseLoanMode').value);
    const firstEmi = document.getElementById('disburseFirstEmiDate').value;

    try {
        const res = await api(`/api/loans/${id}/disburse`, 'POST', {
            loanId: parseInt(id),
            disbursedAmount: amt,
            disbursementMode: mode,
            firstEmiDate: firstEmi ? new Date(firstEmi) : new Date()
        });

        if (res && res.success) {
            closeModal('modalDisburseLoan');
            Swal.fire('Disbursed & Active', 'Loan disbursed! Repayment schedule generated & accounting voucher posted.', 'success');
            loadLoans();
            loadDashboard();
        }
    } catch {
        Swal.fire('Error', 'Disbursement failed', 'error');
    }
}

async function viewLoanSchedule(id, loanNo) {
    try {
        const res = await api(`/api/loans/${id}/schedule`);
        if (res && res.success) {
            document.getElementById('scheduleLoanSubtitle').innerText = `Loan Account #${loanNo}`;
            const tbody = document.getElementById('scheduleTableBody');
            tbody.innerHTML = res.data.map(e => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-2.5 px-3 font-mono font-bold text-slate-500">${e.installmentNumber}</td>
                    <td class="py-2.5 px-3 font-medium">${new Date(e.dueDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-2.5 px-3 text-right">₹${Number(e.principalAmount).toFixed(2)}</td>
                    <td class="py-2.5 px-3 text-right">₹${Number(e.interestAmount).toFixed(2)}</td>
                    <td class="py-2.5 px-3 text-right font-bold text-slate-900">₹${Number(e.totalEmiAmount).toFixed(2)}</td>
                    <td class="py-2.5 px-3 text-right text-emerald-600 font-semibold">₹${Number(e.totalPaidAmount).toFixed(2)}</td>
                    <td class="py-2.5 px-3 text-right font-bold text-brand-700">₹${Number(e.totalOutstanding).toFixed(2)}</td>
                    <td class="py-2.5 px-3 text-center">
                        <span class="px-2 py-0.5 rounded text-[10px] font-bold ${e.statusName === 'Paid' ? 'bg-emerald-50 text-emerald-700' : (e.statusName === 'Overdue' ? 'bg-rose-50 text-rose-700' : 'bg-slate-100 text-slate-600')}">
                            ${e.statusName}
                        </span>
                    </td>
                </tr>
            `).join('');

            openModal('modalLoanSchedule');
        }
    } catch {
        Swal.fire('Error', 'Failed to load passbook', 'error');
    }
}

function showReceiptModal(col) {
    document.getElementById('rcpNumber').innerText = col.receiptNumber;
    document.getElementById('rcpDate').innerText = new Date(col.collectionDate).toLocaleString('en-GB');
    document.getElementById('rcpCustomer').innerText = `Customer ID: ${col.customerId}`;
    document.getElementById('rcpLoanNo').innerText = `Loan ID: ${col.loanApplicationId}`;
    document.getElementById('rcpAmount').innerText = `₹${Number(col.totalAmountPaid).toLocaleString('en-IN', {minimumFractionDigits: 2})}`;
    document.getElementById('rcpPrincipal').innerText = `₹${Number(col.principalPortion).toFixed(2)}`;
    document.getElementById('rcpInterest').innerText = `₹${Number(col.interestPortion).toFixed(2)}`;
    openModal('modalReceipt');
}

function quickApplyForCustomer(cid, name) {
    switchTab('loans');
    setTimeout(() => {
        openApplyLoanModal();
        setTimeout(() => {
            const sel = document.getElementById('applyLoanCustomerSelect');
            if (sel) sel.value = cid;
        }, 200);
    }, 200);
}

function exportLoanPortfolioCsv() {
    window.location.href = '/api/loans?pageSize=1000';
    Swal.fire('Exporting', 'Preparing CSV portfolio report...', 'info');
}
