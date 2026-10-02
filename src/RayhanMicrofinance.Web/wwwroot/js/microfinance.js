// RPS Rayhan Tech Microfinance ERP - Light Corporate Design & Full Page Views (No Popups)

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
        const initial = (currentUser.fullName || 'A').charAt(0).toUpperCase();

        // Topbar Badges (Image 1 & 3)
        const nameTop = document.getElementById('userNameTop');
        const roleTop = document.getElementById('userRoleTextTop');
        const avatarTop = document.getElementById('userAvatarTop');
        if (nameTop) nameTop.innerText = currentUser.fullName.toUpperCase();
        if (roleTop) roleTop.innerText = currentUser.roleName.toUpperCase();
        if (avatarTop) avatarTop.innerText = initial;

        // Drawer Badges (Image 2)
        const drawerName = document.getElementById('drawerUserName');
        const drawerRole = document.getElementById('drawerUserRole');
        const drawerAvatar = document.getElementById('drawerUserAvatar');
        if (drawerName) drawerName.innerText = currentUser.fullName.toUpperCase();
        if (drawerRole) drawerRole.innerText = '• ' + currentUser.roleName.toUpperCase();
        if (drawerAvatar) drawerAvatar.innerText = initial;

        // Fallback elements
        const nameLegacy = document.getElementById('userNameDisplay');
        const roleLegacy = document.getElementById('userRoleDisplay');
        const avatarLegacy = document.getElementById('userAvatar');
        if (nameLegacy) nameLegacy.innerText = currentUser.fullName.toUpperCase();
        if (roleLegacy) roleLegacy.innerText = currentUser.roleName.toUpperCase();
        if (avatarLegacy) avatarLegacy.innerText = initial;

        applyRoleBasedUI();
    }

    await loadCompanyInfo();
    await loadBranches();
    // Switch to dashboard tab — triggers correct sidebar-nav-item active styling and loadDashboard()
    switchTab('dashboard');
}

function applyRoleBasedUI() {
    if (!currentUser) return;
    const isSuperAdminOrAdmin = currentUser.role === 1 || currentUser.role === 2;
    const isFieldOfficer = currentUser.role === 5;
    const isBranchManager = currentUser.role === 3;

    // Clean DB button in topbar
    const cleanDbBtn = document.getElementById('cleanDbBtn');
    if (cleanDbBtn) {
        cleanDbBtn.style.display = isSuperAdminOrAdmin ? 'inline-flex' : 'none';
    }

    // Company Setup button in topbar
    const companySetupBtn = document.getElementById('companySetupBtn');
    if (companySetupBtn) {
        companySetupBtn.style.display = isSuperAdminOrAdmin ? 'inline-flex' : 'none';
    }

    // Sidebar navigation menus
    const navAccounting = document.getElementById('nav-accounting');
    const navExpenses = document.getElementById('nav-expenses');
    const navBranches = document.getElementById('nav-branches');
    const navEmployees = document.getElementById('nav-employees');

    if (isFieldOfficer) {
        if (navAccounting) navAccounting.style.display = 'none';
        if (navExpenses) navExpenses.style.display = 'none';
        if (navBranches) navBranches.style.display = 'none';
        if (navEmployees) navEmployees.style.display = 'none';
    } else if (isBranchManager) {
        if (navBranches) navBranches.style.display = 'none'; // Branch manager cannot create new branches
        if (navAccounting) navAccounting.style.display = 'flex';
        if (navExpenses) navExpenses.style.display = 'flex';
        if (navEmployees) navEmployees.style.display = 'flex';
    } else {
        if (navAccounting) navAccounting.style.display = 'flex';
        if (navExpenses) navExpenses.style.display = 'flex';
        if (navBranches) navBranches.style.display = 'flex';
        if (navEmployees) navEmployees.style.display = 'flex';
    }
}

// 3. BRANCH SELECTOR
async function loadBranches() {
    try {
        const res = await api('/api/branches');
        if (res && res.success) {
            const selector = document.getElementById('branchSelector');
            const isSuperAdminOrAdmin = currentUser && (currentUser.role === 1 || currentUser.role === 2);

            if (!isSuperAdminOrAdmin && currentUser && currentUser.branchId) {
                selector.innerHTML = '';
                res.data.forEach(b => {
                    if (b.id === currentUser.branchId) {
                        const opt = document.createElement('option');
                        opt.value = b.id;
                        opt.innerText = `${b.branchCode} - ${b.branchName.toUpperCase()}`;
                        selector.appendChild(opt);
                    }
                });
                selector.value = currentUser.branchId;
                selectedBranchId = currentUser.branchId;
                selector.disabled = true;
            } else {
                selector.disabled = false;
                selector.innerHTML = '<option value="">ALL BRANCHES (CONSOLIDATED)</option>';
                res.data.forEach(b => {
                    const opt = document.createElement('option');
                    opt.value = b.id;
                    opt.innerText = `${b.branchCode} - ${b.branchName.toUpperCase()}`;
                    selector.appendChild(opt);
                });
                if (selectedBranchId) selector.value = selectedBranchId;
            }

            // Populate branch cards
            const container = document.getElementById('branchesCardsContainer');
            if (container) {
                container.innerHTML = res.data.map(b => `
                    <div class="bg-white rounded-3xl p-6 border-2 border-slate-200/90 shadow-md space-y-4">
                        <div class="flex items-center justify-between">
                            <span class="text-xs font-black px-3 py-1 bg-emerald-50 text-emerald-800 rounded-xl border border-emerald-200">${b.branchCode}</span>
                            <span class="text-xs font-black uppercase text-slate-400">${b.isHeadOffice ? 'HEAD OFFICE' : 'REGIONAL BRANCH'}</span>
                        </div>
                        <h4 class="font-black text-slate-900 text-base uppercase">${b.branchName}</h4>
                        <p class="text-xs font-bold text-slate-500 uppercase">${b.address}, ${b.city}, ${b.state} - ${b.pincode}</p>
                        <div class="pt-4 border-t-2 border-slate-100 grid grid-cols-2 gap-3 text-xs">
                            <div>
                                <span class="text-slate-400 block text-[10px] uppercase font-black">VAULT CASH</span>
                                <span class="font-black text-slate-900 text-sm font-mono">₹${Number(b.currentCashBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                            <div>
                                <span class="text-slate-400 block text-[10px] uppercase font-black">BANK CURRENT BAL</span>
                                <span class="font-black text-blue-700 text-sm font-mono">₹${Number(b.currentBankBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
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

// 3.1 SIDEBAR CONTROLS (LIGHT COLLAPSIBLE DRAWER)
function openSidebar() {
    const sb = document.getElementById('sidebar');
    const bd = document.getElementById('sidebarBackdrop');
    if (sb) {
        sb.classList.remove('-translate-x-full');
        sb.classList.add('translate-x-0');
    }
    if (bd) {
        bd.classList.remove('opacity-0', 'pointer-events-none');
        bd.classList.add('opacity-100', 'pointer-events-auto');
    }
}

function closeSidebar() {
    const sb = document.getElementById('sidebar');
    const bd = document.getElementById('sidebarBackdrop');
    if (sb) {
        sb.classList.remove('translate-x-0');
        sb.classList.add('-translate-x-full');
    }
    if (bd) {
        bd.classList.remove('opacity-100', 'pointer-events-auto');
        bd.classList.add('opacity-0', 'pointer-events-none');
    }
}

function toggleSidebar() {
    const sb = document.getElementById('sidebar');
    if (sb && sb.classList.contains('translate-x-0')) {
        closeSidebar();
    } else {
        openSidebar();
    }
}

// Close sidebar on Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeSidebar();
});

// 4. FULL-PAGE TAB SWITCHER (NO POPUPS)
function switchTab(tab) {
    activeTab = tab;

    // Automatically close sidebar drawer on navigation so user gets full workspace
    closeSidebar();

    // Hide all tab views, then show the target one
    document.querySelectorAll('.tab-view').forEach(v => v.classList.add('hidden'));
    const target = document.getElementById(`view-${tab}`);
    if (target) target.classList.remove('hidden');

    // Update Sidebar Navigation active state
    document.querySelectorAll('.sidebar-nav-item, .sidebar-menu-card').forEach(el => {
        el.classList.remove('active');
    });
    const activeNavItem = document.getElementById(`nav-${tab}`);
    if (activeNavItem) {
        activeNavItem.classList.add('active');
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
    else if (tab === 'branches') loadBranches();
}

// 5. DASHBOARD (IMAGE 1 DESIGN)
async function loadDashboard() {
    try {
        const query = selectedBranchId ? `?branchId=${selectedBranchId}` : '';
        const res = await api(`/api/dashboard/summary${query}`);
        if (res && res.success) {
            const d = res.data;

            // Row 1: Primary colorful stat cards
            const setTxt = (id, txt) => {
                const el = document.getElementById(id);
                if (el) el.innerText = txt;
            };

            setTxt('statTotalCustomers', d.totalCustomersCount || 0);
            setTxt('statActiveLoansCount', d.activeLoansCount || 0);
            setTxt('statTodayCollection', `₹${Number(d.todayCollection || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statTotalPortfolio', `₹${Number(d.totalActiveLoanPortfolio || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statTotalSavings', `₹${Number(d.totalSavingsBalance || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statTodayDisbursement', `₹${Number(d.todayDisbursement || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statOverdueAmount', `₹${Number(d.overdueAmount || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statOverdueCount', d.overdueLoansCount || 0);

            // Row 2: Secondary pink stat cards
            setTxt('statCashBalance', `₹${Number(d.totalCashBalance || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statBankBalance', `₹${Number(d.totalBankBalance || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            
            const dayCashIn = (d.todayCollection || 0);
            const dayCashOut = (d.todayDisbursement || 0);
            setTxt('statDayCashIn', `₹${Number(dayCashIn).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statDayCashOut', `₹${Number(dayCashOut).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statNetProfit', `₹${Number(d.netProfit || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`);
            setTxt('statTotalBranches', d.branchPerformances ? d.branchPerformances.length : 2);
            setTxt('statTotalStaff', 5);

            // Render Chart
            renderMonthlyChart(d.monthlyDisbursementAndCollection || []);

            // Render Branch Performance (Image 1 Ranking Table)
            const perfContainer = document.getElementById('branchPerformanceList');
            if (perfContainer) {
                if (!d.branchPerformances || d.branchPerformances.length === 0) {
                    perfContainer.innerHTML = '<div class="text-center py-4 text-slate-400 text-xs font-bold uppercase">No branch data available</div>';
                } else {
                    perfContainer.innerHTML = d.branchPerformances.map((p, idx) => `
                        <div class="p-3.5 bg-slate-50 rounded-2xl border-2 border-slate-200/80 flex items-center justify-between hover:bg-slate-100 transition">
                            <div class="flex items-center gap-3">
                                <span class="w-7 h-7 rounded-xl bg-blue-600 text-white font-black text-xs flex items-center justify-center shadow-xs">#${idx + 1}</span>
                                <div>
                                    <p class="font-black text-xs text-slate-900 uppercase">${p.branchName}</p>
                                    <p class="text-[10px] font-bold text-slate-400 uppercase">${p.activeLoans} ACTIVE LOANS IN PORTFOLIO</p>
                                </div>
                            </div>
                            <div class="text-right">
                                <p class="font-black text-xs text-blue-900 font-mono">₹${Number(p.outstandingAmount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</p>
                                <span class="text-[9px] font-black text-emerald-800 bg-emerald-100 px-2.5 py-0.5 rounded-full border border-emerald-300 uppercase">
                                    ${p.collectionEfficiencyPercentage || 98.5}% RECOVERY
                                </span>
                            </div>
                        </div>
                    `).join('');
                }
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

    const labels = dataPoints.map(d => d.monthName.toUpperCase());
    const disbData = dataPoints.map(d => d.disbursement);
    const colData = dataPoints.map(d => d.collection);

    monthlyChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'DISBURSEMENT (₹)',
                    data: disbData,
                    backgroundColor: 'rgba(59, 130, 246, 0.85)',
                    borderRadius: 8
                },
                {
                    label: 'COLLECTION (₹)',
                    data: colData,
                    backgroundColor: 'rgba(16, 185, 129, 0.9)',
                    borderRadius: 8
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top', labels: { boxWidth: 12, font: { family: 'Inter', weight: 'bold', size: 11 } } }
            },
            scales: {
                y: { beginAtZero: true, grid: { color: '#e2e8f0' } },
                x: { grid: { display: false } }
            }
        }
    });
}

// 6. CUSTOMERS (FULL PAGE VIEW - IMAGE 3 DESIGN)
async function loadCustomers() {
    try {
        const search = document.getElementById('custSearchInput')?.value || '';
        const branchFilter = document.getElementById('custFilterBranch')?.value || selectedBranchId || '';
        let url = `/api/customers?pageIndex=1&pageSize=100`;
        if (branchFilter) url += `&branchId=${branchFilter}`;
        if (search) url += `&search=${encodeURIComponent(search)}`;

        const res = await api(url);
        const tbody = document.getElementById('customersTableBody');
        if (res && res.success && tbody) {
            const items = res.data.items || [];

            // Populate branch filter options if not populated
            const branchSelect = document.getElementById('custFilterBranch');
            if (branchSelect && branchSelect.options.length <= 1) {
                const bRes = await api('/api/branches');
                if (bRes && bRes.success) {
                    branchSelect.innerHTML = '<option value="">ALL BRANCHES</option>' + 
                        bRes.data.map(b => `<option value="${b.id}">${b.branchCode} - ${b.branchName.toUpperCase()}</option>`).join('');
                    if (branchFilter) branchSelect.value = branchFilter;
                }
            }

            // Calculate 7 customer stats (Image 3)
            const total = res.data.totalCount || items.length;
            const active = items.filter(c => !c.isBlacklisted).length;
            const borrowers = items.filter(c => c.activeLoansCount > 0 || c.hasActiveLoan || !c.isBlacklisted).length;
            const verifiedKyc = items.filter(c => c.isKycVerified).length;
            const pendingKyc = items.filter(c => !c.isKycVerified).length;
            const unassigned = items.filter(c => !c.centerName || c.centerName === 'Direct').length;
            const inactive = items.filter(c => c.isBlacklisted).length;

            const setTxt = (id, txt) => {
                const el = document.getElementById(id);
                if (el) el.innerText = txt;
            };
            setTxt('custStatTotal', total);
            setTxt('custStatActive', active);
            setTxt('custStatBorrowers', borrowers);
            setTxt('custStatKycVerified', verifiedKyc);
            setTxt('custStatKycPending', pendingKyc);
            setTxt('custStatUnassigned', unassigned);
            setTxt('custStatInactive', inactive);

            if (items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center py-8 text-slate-400 font-black uppercase">No member records found. Click "+ Add" to register new borrower.</td></tr>';
                return;
            }

            // Render table rows matching Image 3 (circular avatar badge, clean pill tags)
            tbody.innerHTML = items.map((c, i) => {
                const initial = (c.fullName || 'M').charAt(0).toUpperCase();
                const avatarColors = ['bg-blue-600', 'bg-emerald-600', 'bg-purple-600', 'bg-amber-600', 'bg-rose-600', 'bg-teal-600'];
                const avBg = avatarColors[i % avatarColors.length];

                return `
                <tr class="hover:bg-blue-50/50 transition">
                    <td class="py-3 px-4 font-mono font-black text-blue-900">${c.customerCode}</td>
                    <td class="py-3 px-4">
                        <div class="flex items-center gap-2.5">
                            <div class="w-8 h-8 rounded-full ${avBg} text-white flex items-center justify-center font-black text-xs flex-shrink-0 shadow-xs">
                                ${initial}
                            </div>
                            <div>
                                <div class="font-black text-slate-900 uppercase">${c.fullName}</div>
                                <div class="text-[10px] text-slate-400 uppercase font-bold">${c.relationWithGuardian || 'Relation'}: ${c.guardianName || '--'}</div>
                            </div>
                        </div>
                    </td>
                    <td class="py-3 px-4 font-mono font-bold text-slate-700">
                        <div class="flex items-center gap-1.5"><i class="fa-solid fa-phone text-slate-400 text-[10px]"></i><span>${c.phone}</span></div>
                    </td>
                    <td class="py-3 px-4">
                        <span class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase bg-blue-50 text-blue-800 border border-blue-200">${c.branchName || 'BRANCH'}</span>
                        <div class="text-[10px] text-slate-500 uppercase font-bold mt-0.5">${c.centerName || 'Direct'}${c.groupName ? ' • ' + c.groupName : ''}</div>
                    </td>
                    <td class="py-3 px-4 font-mono">
                        <div class="font-bold text-slate-800">${c.aadhaarNumber || '--'}</div>
                        ${c.isKycVerified ? `
                        <span class="inline-flex items-center text-[9px] font-black text-emerald-800 bg-emerald-50 px-2 py-0.5 rounded-full border border-emerald-200 uppercase mt-0.5">
                            <i class="fa-solid fa-circle-check mr-1 text-[9px] text-emerald-600"></i> KYC VERIFIED
                        </span>
                        ` : `
                        <span class="inline-flex items-center text-[9px] font-black text-amber-800 bg-amber-50 px-2 py-0.5 rounded-full border border-amber-200 uppercase mt-0.5">
                            <i class="fa-solid fa-clock mr-1 text-[9px] text-amber-600"></i> PENDING KYC
                        </span>
                        `}
                    </td>
                    <td class="py-3 px-4 uppercase font-bold text-slate-600">${c.occupation || '--'}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-2.5 py-1 rounded-full text-[10px] font-black uppercase ${c.isBlacklisted ? 'bg-rose-50 text-rose-700 border border-rose-200' : 'bg-emerald-50 text-emerald-800 border border-emerald-200'}">
                            ${c.isBlacklisted ? 'BLACKLISTED' : 'ACTIVE'}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right">
                        <div class="flex items-center justify-end gap-1.5 flex-wrap">
                            <button onclick="openCustomerDocs(${c.id})" class="px-2.5 py-1.5 bg-amber-500 hover:bg-amber-600 text-white rounded-xl text-[11px] font-black uppercase shadow-xs transition cursor-pointer flex items-center gap-1" title="Upload KYC Documents">
                                <i class="fa-solid fa-file-arrow-up"></i> DOCS
                            </button>
                            ${!c.isKycVerified ? `
                            <button onclick="verifyCustomerKycDirect(${c.id}, '${c.fullName.replace(/'/g, "\\'")}')" class="px-2.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-[11px] font-black uppercase shadow-xs transition cursor-pointer flex items-center gap-1" title="Verify KYC">
                                <i class="fa-solid fa-check"></i> VERIFY
                            </button>
                            ` : ''}
                            <button onclick="quickApplyForCustomer(${c.id}, '${c.fullName.replace(/'/g, "\\'")}')" class="px-2.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-[11px] font-black uppercase shadow-xs transition cursor-pointer flex items-center gap-1">
                                <i class="fa-solid fa-plus"></i> LOAN
                            </button>
                        </div>
                    </td>
                </tr>
            `}).join('');
        }
    } catch (err) {
        console.error('Error loading customers', err);
    }
}

async function exportCustomersCsv() {
    try {
        const res = await api('/api/customers?pageIndex=1&pageSize=1000');
        if (res && res.success && res.data.items) {
            const items = res.data.items;
            const headers = ['Member ID', 'Full Name', 'Guardian', 'Phone', 'Aadhaar', 'Branch', 'Center', 'Group', 'Occupation', 'Status'];
            const rows = items.map(c => [
                c.customerCode,
                `"${(c.fullName || '').replace(/"/g, '""')}"`,
                `"${(c.guardianName || '').replace(/"/g, '""')}"`,
                c.phone,
                c.aadhaarNumber || '',
                `"${(c.branchName || '').replace(/"/g, '""')}"`,
                `"${(c.centerName || '').replace(/"/g, '""')}"`,
                `"${(c.groupName || '').replace(/"/g, '""')}"`,
                `"${(c.occupation || '').replace(/"/g, '""')}"`,
                c.isBlacklisted ? 'BLACKLISTED' : 'ACTIVE'
            ]);
            const csv = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\r\n');
            const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.setAttribute('download', `customers_directory_${new Date().toISOString().slice(0,10)}.csv`);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    } catch (err) {
        Swal.fire('Export Error', 'Failed to export customer directory.', 'error');
    }
}

function searchCustomers() {
    clearTimeout(window.searchTimer);
    window.searchTimer = setTimeout(loadCustomers, 300);
}

// Dedicated full page show for member registration
async function showRegisterMemberPage() {
    const cRes = await api('/api/centers');
    const sel = document.getElementById('newCustCenter');
    if (cRes && cRes.success && sel) {
        sel.innerHTML = cRes.data.map(c => `<option value="${c.id}">${c.centerName.toUpperCase()}</option>`).join('');
    }
    switchTab('customer-register');
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
            Swal.fire({
                title: 'MEMBER REGISTERED',
                html: `<div class="text-left font-bold text-xs space-y-2">
                         <p>MEMBER CODE: <span class="text-blue-600 font-mono">${res.data.customerCode}</span></p>
                         <p class="text-amber-800 bg-amber-50 p-2.5 rounded-xl border border-amber-200">
                           <i class="fa-solid fa-clock mr-1 text-amber-600"></i> STATUS: <strong>PENDING KYC</strong>
                         </p>
                         <p class="text-slate-500 text-[11px]">Member registered in pending KYC state. Please upload identity documents and verify eligibility.</p>
                       </div>`,
                icon: 'success',
                showCancelButton: true,
                confirmButtonText: 'UPLOAD DOCS NOW',
                cancelButtonText: 'VIEW IN DIRECTORY',
                confirmButtonColor: '#059669',
                cancelButtonColor: '#64748b'
            }).then((result) => {
                if (result.isConfirmed && res.data.id) {
                    openCustomerDocs(res.data.id);
                } else {
                    switchTab('customers');
                }
            });
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Failed to register member', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
}

// 6.1 CUSTOMER KYC & DOCUMENT MANAGEMENT (FULL PAGE VIEW, ZERO POPUPS)
let currentDocCustomer = null;

async function openCustomerDocs(customerId) {
    try {
        const res = await api(`/api/customers/${customerId}`);
        if (!res || !res.success) {
            Swal.fire('Error', 'Unable to load member documents.', 'error');
            return;
        }

        currentDocCustomer = res.data;
        const c = currentDocCustomer;

        // Switch to the dedicated document page
        document.querySelectorAll('.tab-view').forEach(v => v.classList.add('hidden'));
        const docView = document.getElementById('view-customer-documents');
        if (docView) docView.classList.remove('hidden');

        // Populate Member Banner Profile
        const initial = (c.fullName || 'M').charAt(0).toUpperCase();
        const avatarEl = document.getElementById('docCustAvatar');
        if (avatarEl) avatarEl.innerText = initial;

        const setTxt = (id, val) => {
            const el = document.getElementById(id);
            if (el) el.innerText = val || '--';
        };

        setTxt('docCustName', c.fullName);
        setTxt('docCustCode', c.customerCode);
        setTxt('docCustGuardian', `${c.relationWithGuardian || 'SPOUSE / FATHER'}: ${c.guardianName || '--'}`);
        setTxt('docCustPhone', c.phone);
        setTxt('docCustBranch', c.branchName || 'HEAD OFFICE');
        setTxt('docCustCenter', `${c.centerName || 'Direct'}${c.groupName ? ' • ' + c.groupName : ''}`);
        setTxt('docCustAadhaar', c.aadhaarNumber || 'NOT PROVIDED');

        // KYC Status Badges
        const kycBadge = document.getElementById('docCustKycBadge');
        const verifyBtn = document.getElementById('btnVerifyCustomerKycAction');

        if (c.isKycVerified) {
            if (kycBadge) {
                kycBadge.className = 'text-[10px] font-black uppercase px-2.5 py-0.5 rounded-full bg-emerald-50 text-emerald-800 border border-emerald-200';
                kycBadge.innerHTML = '<i class="fa-solid fa-circle-check mr-1 text-emerald-600"></i> KYC VERIFIED';
            }
            if (verifyBtn) {
                verifyBtn.className = 'px-5 py-2.5 bg-slate-200 text-slate-700 text-xs font-black uppercase tracking-wider rounded-2xl transition flex items-center cursor-pointer';
                verifyBtn.innerHTML = '<i class="fa-solid fa-check mr-1.5 text-sm text-emerald-600"></i> VERIFIED';
            }
        } else {
            if (kycBadge) {
                kycBadge.className = 'text-[10px] font-black uppercase px-2.5 py-0.5 rounded-full bg-amber-50 text-amber-800 border border-amber-200';
                kycBadge.innerHTML = '<i class="fa-solid fa-clock mr-1 text-amber-600"></i> PENDING KYC';
            }
            if (verifyBtn) {
                verifyBtn.className = 'px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-black uppercase tracking-wider rounded-2xl shadow-md transition flex items-center cursor-pointer';
                verifyBtn.innerHTML = '<i class="fa-solid fa-circle-check mr-1.5 text-sm"></i> APPROVE & VERIFY KYC';
            }
        }

        // 1. Aadhaar Front
        const afStatus = document.getElementById('docStatusAadhaarFront');
        const afPreview = document.getElementById('previewAadhaarFront');
        if (c.kycDocumentFrontUrl) {
            if (afStatus) {
                afStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300';
                afStatus.innerText = 'UPLOADED';
            }
            if (afPreview) {
                afPreview.innerHTML = `<img src="${c.kycDocumentFrontUrl}" class="w-full h-full object-cover rounded-lg" alt="Aadhaar Front" />`;
            }
        } else {
            if (afStatus) {
                afStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-slate-200 text-slate-600';
                afStatus.innerText = 'MISSING';
            }
            if (afPreview) {
                afPreview.innerHTML = `<i class="fa-solid fa-image text-2xl text-slate-300 mb-1"></i><span class="text-[10px] font-bold text-slate-400 uppercase">NO FILE SELECTED</span>`;
            }
        }

        // 2. Aadhaar Back
        const abStatus = document.getElementById('docStatusAadhaarBack');
        const abPreview = document.getElementById('previewAadhaarBack');
        if (c.kycDocumentBackUrl) {
            if (abStatus) {
                abStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300';
                abStatus.innerText = 'UPLOADED';
            }
            if (abPreview) {
                abPreview.innerHTML = `<img src="${c.kycDocumentBackUrl}" class="w-full h-full object-cover rounded-lg" alt="Aadhaar Back" />`;
            }
        } else {
            if (abStatus) {
                abStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-slate-200 text-slate-600';
                abStatus.innerText = 'MISSING';
            }
            if (abPreview) {
                afPreview.innerHTML = `<i class="fa-solid fa-image text-2xl text-slate-300 mb-1"></i><span class="text-[10px] font-bold text-slate-400 uppercase">NO FILE SELECTED</span>`;
            }
        }

        // 3. PAN / Voter
        const panStatus = document.getElementById('docStatusPan');
        const panInput = document.getElementById('docPanNumberInput');
        if (panInput) panInput.value = c.panNumber || c.voterIdNumber || '';
        if (c.panNumber || c.voterIdNumber) {
            if (panStatus) {
                panStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300';
                panStatus.innerText = 'REGISTERED';
            }
        } else {
            if (panStatus) {
                panStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-slate-200 text-slate-600';
                panStatus.innerText = 'MISSING';
            }
        }

        // 4. Photo
        const photoStatus = document.getElementById('docStatusPhoto');
        const photoPreview = document.getElementById('previewPhoto');
        if (c.photoUrl) {
            if (photoStatus) {
                photoStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300';
                photoStatus.innerText = 'UPLOADED';
            }
            if (photoPreview) {
                photoPreview.innerHTML = `<img src="${c.photoUrl}" class="w-full h-full object-cover rounded-lg" alt="Member Photo" />`;
            }
        } else {
            if (photoStatus) {
                photoStatus.className = 'text-[9px] font-black uppercase px-2 py-0.5 rounded-full bg-slate-200 text-slate-600';
                photoStatus.innerText = 'MISSING';
            }
            if (photoPreview) {
                photoPreview.innerHTML = `<i class="fa-solid fa-user text-2xl text-slate-300 mb-1"></i><span class="text-[10px] font-bold text-slate-400 uppercase">NO PHOTO</span>`;
            }
        }

        // Clear file inputs
        ['fileAadhaarFront', 'fileAadhaarBack', 'filePan', 'filePhoto'].forEach(id => {
            const input = document.getElementById(id);
            if (input) input.value = '';
        });

    } catch (err) {
        console.error('Error opening customer docs', err);
        Swal.fire('Error', 'Unable to retrieve member documents.', 'error');
    }
}

function previewSelectedDoc(inputId, previewContainerId) {
    const input = document.getElementById(inputId);
    const container = document.getElementById(previewContainerId);
    if (!input || !input.files || input.files.length === 0 || !container) return;

    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = function(e) {
        if (file.type.startsWith('image/')) {
            container.innerHTML = `<img src="${e.target.result}" class="w-full h-full object-contain rounded-lg shadow-inner" />`;
        } else {
            container.innerHTML = `<div class="text-center p-2"><i class="fa-solid fa-file-pdf text-3xl text-rose-500 mb-1"></i><p class="text-[10px] font-black text-slate-700 uppercase truncate">${file.name}</p></div>`;
        }
    };
    reader.readAsDataURL(file);
}

async function handleDocUpload(docType, fileInputId, numberInputId = null) {
    if (!currentDocCustomer || !currentDocCustomer.id) {
        Swal.fire('Error', 'No customer currently selected.', 'error');
        return;
    }

    const fileInput = document.getElementById(fileInputId);
    let base64 = null;
    let fileName = null;

    if (fileInput && fileInput.files && fileInput.files.length > 0) {
        const file = fileInput.files[0];
        fileName = file.name;
        base64 = await new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    let docNumber = null;
    if (numberInputId) {
        const numInput = document.getElementById(numberInputId);
        if (numInput) docNumber = numInput.value.trim();
    }

    if (!base64 && !docNumber) {
        Swal.fire('Select File', 'Please select a document file or enter the number to upload.', 'warning');
        return;
    }

    try {
        const res = await api(`/api/customers/${currentDocCustomer.id}/upload-document`, 'POST', {
            documentType: docType,
            documentNumber: docNumber,
            base64Data: base64,
            fileName: fileName
        });

        if (res && res.success) {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: 'DOCUMENT UPLOADED',
                showConfirmButton: false,
                timer: 2500
            });
            await openCustomerDocs(currentDocCustomer.id);
            loadCustomers();
        } else {
            Swal.fire('Upload Failed', res?.message || 'Could not upload document', 'error');
        }
    } catch {
        Swal.fire('Error', 'Network or server error during upload.', 'error');
    }
}

async function toggleCustomerKycStatus(isVerified) {
    if (!currentDocCustomer || !currentDocCustomer.id) return;

    try {
        const res = await api(`/api/customers/${currentDocCustomer.id}/verify-kyc?isVerified=${isVerified}`, 'POST');
        if (res && res.success) {
            Swal.fire({
                title: isVerified ? 'KYC VERIFIED & APPROVED' : 'KYC SET TO PENDING',
                text: res.message || 'Status successfully updated',
                icon: isVerified ? 'success' : 'info',
                confirmButtonColor: '#059669'
            });
            await openCustomerDocs(currentDocCustomer.id);
            loadCustomers();
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Failed to update KYC status', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure while updating verification status.', 'error');
    }
}

function submitVerifyCurrentCustomer() {
    if (!currentDocCustomer || !currentDocCustomer.id) return;
    if (currentDocCustomer.isKycVerified) {
        Swal.fire({
            title: 'ALREADY VERIFIED',
            text: 'This member has already been KYC verified and approved.',
            icon: 'info'
        });
        return;
    }
    toggleCustomerKycStatus(true);
}

async function verifyCustomerKycDirect(id, memberName) {
    const confirmResult = await Swal.fire({
        title: 'VERIFY MEMBER KYC?',
        html: `<div class="text-left font-bold text-xs space-y-2">
                 <p>Member: <strong class="text-slate-900 uppercase">${memberName}</strong></p>
                 <p class="text-emerald-800 bg-emerald-50 p-2.5 rounded-xl border border-emerald-200">
                   <i class="fa-solid fa-shield-check mr-1.5 text-emerald-600"></i> By confirming, you verify that the member identity and documentation are valid and approve them for microfinance loans.
                 </p>
               </div>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '✓ YES, APPROVE & VERIFY',
        cancelButtonText: 'CANCEL',
        confirmButtonColor: '#059669',
        cancelButtonColor: '#64748b'
    });

    if (!confirmResult.isConfirmed) return;

    try {
        const res = await api(`/api/customers/${id}/verify-kyc?isVerified=true`, 'POST');
        if (res && res.success) {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: `KYC VERIFIED: ${memberName}`,
                showConfirmButton: false,
                timer: 3000
            });
            loadCustomers();
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Verification failed', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
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
            document.getElementById('centerCountBadge').innerText = `${cRes.data.length} CENTERS`;
            const container = document.getElementById('centersListContainer');
            container.innerHTML = cRes.data.map(c => `
                <div class="p-4 bg-slate-50 rounded-2xl border-2 border-slate-200/80 flex items-center justify-between">
                    <div>
                        <div class="flex items-center gap-2">
                            <span class="font-black text-xs text-slate-900 uppercase">${c.centerName}</span>
                            <span class="text-[10px] font-mono font-black bg-slate-200 text-slate-800 px-2 py-0.5 rounded-lg">${c.centerCode}</span>
                        </div>
                        <p class="text-[11px] font-bold text-slate-500 uppercase mt-1">MEETING: <span class="text-slate-800">${c.meetingDayName} AT ${c.meetingTime}</span> | PLACE: ${c.meetingPlace}</p>
                    </div>
                    <div class="text-right">
                        <span class="text-xs font-black text-emerald-800 uppercase block">${c.totalGroups} GROUPS</span>
                        <span class="text-[11px] font-black text-slate-400 uppercase">${c.totalMembers} MEMBERS</span>
                    </div>
                </div>
            `).join('');
        }

        if (gRes && gRes.success) {
            document.getElementById('groupCountBadge').innerText = `${gRes.data.length} GROUPS`;
            const container = document.getElementById('groupsListContainer');
            container.innerHTML = gRes.data.map(g => `
                <div class="p-4 bg-slate-50 rounded-2xl border-2 border-slate-200/80 flex items-center justify-between">
                    <div>
                        <div class="flex items-center gap-2">
                            <span class="font-black text-xs text-slate-900 uppercase">${g.groupName}</span>
                            <span class="text-[10px] font-mono font-black bg-blue-100 text-blue-800 px-2 py-0.5 rounded-lg">${g.groupCode}</span>
                        </div>
                        <p class="text-[11px] font-bold text-slate-500 uppercase mt-1">CENTER: <span class="text-slate-800">${g.centerName}</span> | LEADER: ${g.groupLeaderName || 'NOT ASSIGNED'}</p>
                    </div>
                    <div class="text-right">
                        <span class="text-xs font-black text-blue-800 uppercase block">${g.memberCount} / 5 MEMBERS</span>
                        <span class="text-[10px] text-emerald-700 font-black uppercase">ACTIVE JLG</span>
                    </div>
                </div>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading centers', err);
    }
}

// 8. LOANS (FULL PAGE SCREENS)
let currentLoanFilter = '';

async function loadLoans() {
    try {
        const sRes = await api('/api/loans/schemes');
        if (sRes && sRes.success) {
            const container = document.getElementById('schemesCardsContainer');
            container.innerHTML = sRes.data.map(s => `
                <div class="bg-white rounded-3xl p-5 border-2 border-slate-200/90 shadow-md space-y-2">
                    <div class="flex justify-between items-center">
                        <span class="text-[10px] font-mono font-black bg-emerald-50 text-emerald-800 px-2 py-0.5 rounded-lg border border-emerald-200">${s.schemeCode}</span>
                        <span class="text-xs font-black text-emerald-700">${s.interestRatePerAnnum}% P.A.</span>
                    </div>
                    <h5 class="font-black text-xs text-slate-900 uppercase leading-snug">${s.schemeName}</h5>
                    <p class="text-[11px] font-bold text-slate-500 uppercase">MIN: ₹${s.minAmount.toLocaleString()} | MAX: ₹${s.maxAmount.toLocaleString()}</p>
                    <div class="text-[10px] text-slate-400 font-black uppercase">${s.repaymentFrequency == 2 ? 'WEEKLY' : 'MONTHLY'} (${s.interestCalculationMethod == 1 ? 'FLAT' : 'REDUCING'})</div>
                </div>
            `).join('');
        }

        let url = `/api/loans?pageIndex=1&pageSize=50`;
        if (selectedBranchId) url += `&branchId=${selectedBranchId}`;
        if (currentLoanFilter) url += `&status=${currentLoanFilter}`;

        const res = await api(url);
        const tbody = document.getElementById('loansTableBody');
        if (res && res.success && tbody) {
            if (res.data.items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="9" class="text-center py-8 text-slate-400 font-black uppercase">No loan records found. Click "+ Apply New Loan" to create one.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.items.map(l => `
                <tr class="hover:bg-emerald-50/40 transition">
                    <td class="py-3 px-4 font-mono font-black text-emerald-800">${l.loanAccountNumber}</td>
                    <td class="py-3 px-4">
                        <div class="font-black text-slate-900 uppercase">${l.customerName}</div>
                        <div class="text-[11px] text-slate-400 font-mono font-bold">${l.customerPhone || ''}</div>
                    </td>
                    <td class="py-3 px-4">
                        <div class="font-black text-slate-800 uppercase">${l.loanSchemeName}</div>
                        <div class="text-[10px] font-bold text-slate-400 uppercase">${l.tenureInMonths} MONTHS | ${l.frequency == 2 ? 'WEEKLY' : 'MONTHLY'}</div>
                    </td>
                    <td class="py-3 px-4 text-right font-black text-slate-900 font-mono text-sm">₹${Number(l.disbursedAmount > 0 ? l.disbursedAmount : l.approvedAmount > 0 ? l.approvedAmount : l.requestedAmount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold">₹${Number(l.totalPayable).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-emerald-700 font-mono font-bold">₹${Number(l.totalPaid).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right text-slate-900 font-mono font-black">₹${Number(l.totalOutstanding).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-3 py-1 rounded-full text-[10px] font-black uppercase ${getLoanStatusBadge(l.statusName)}">
                            ${l.statusName}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right space-x-1.5">
                        ${l.statusName === 'Applied' ? (
                            (currentUser && (currentUser.role === 1 || currentUser.role === 2 || currentUser.role === 3))
                            ? `<button onclick="directApproveLoan(${l.id}, '${l.loanAccountNumber}', ${l.requestedAmount})" class="px-3 py-1.5 bg-blue-50 text-blue-800 hover:bg-blue-100 rounded-xl text-xs font-black uppercase border border-blue-200">APPROVE</button>`
                            : `<span class="text-[10px] font-black text-amber-600 uppercase bg-amber-50 px-2.5 py-1 rounded-lg border border-amber-200">PENDING APPROVAL</span>`
                        ) : ''}
                        ${l.statusName === 'Approved' ? (
                            (currentUser && (currentUser.role === 1 || currentUser.role === 2 || currentUser.role === 3))
                            ? `<button onclick="directDisburseLoan(${l.id}, '${l.loanAccountNumber}', ${l.approvedAmount})" class="px-3 py-1.5 bg-emerald-50 text-emerald-800 hover:bg-emerald-100 rounded-xl text-xs font-black uppercase border border-emerald-200">DISBURSE</button>`
                            : `<span class="text-[10px] font-black text-blue-600 uppercase bg-blue-50 px-2.5 py-1 rounded-lg border border-blue-200">READY FOR DISBURSEMENT</span>`
                        ) : ''}
                        ${l.statusName === 'Active' || l.statusName === 'Closed' ? `
                            <button onclick="viewLoanSchedule(${l.id}, '${l.loanAccountNumber}')" class="px-3 py-1.5 bg-slate-100 text-slate-800 hover:bg-slate-200 rounded-xl text-xs font-black uppercase border border-slate-300">
                                <i class="fa-solid fa-list-ol mr-1"></i> PASSBOOK
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
        case 'Applied': return 'bg-amber-50 text-amber-800 border border-amber-200';
        case 'Approved': return 'bg-blue-50 text-blue-800 border border-blue-200';
        case 'Active': return 'bg-emerald-50 text-emerald-800 border border-emerald-200';
        case 'Closed': return 'bg-slate-100 text-slate-700 border border-slate-300';
        default: return 'bg-slate-100 text-slate-700';
    }
}

function filterLoansByStatus(status) {
    currentLoanFilter = status;
    document.querySelectorAll('.loan-filter-btn').forEach(b => {
        b.classList.remove('bg-emerald-600', 'text-white');
        b.classList.add('text-slate-600');
    });
    event.target.classList.add('bg-emerald-600', 'text-white');
    event.target.classList.remove('text-slate-600');
    loadLoans();
}

// Show dedicated Full Page Loan Application
async function showApplyLoanPage() {
    const [cRes, sRes] = await Promise.all([
        api('/api/customers?pageSize=100'),
        api('/api/loans/schemes')
    ]);

    const cSel = document.getElementById('applyLoanCustomerSelect');
    if (cRes && cRes.success && cSel) {
        cSel.innerHTML = cRes.data.items.map(c => `<option value="${c.id}">${c.customerCode} - ${c.fullName.toUpperCase()} (${c.phone})</option>`).join('');
    }

    const sSel = document.getElementById('applyLoanSchemeSelect');
    if (sRes && sRes.success && sSel) {
        window.loadedSchemes = sRes.data;
        sSel.innerHTML = sRes.data.map(s => `<option value="${s.id}">${s.schemeName.toUpperCase()} (${s.interestRatePerAnnum}% P.A.)</option>`).join('');
        onSchemeChange();
    }

    switchTab('loan-apply');
}

function onSchemeChange() {
    const sId = parseInt(document.getElementById('applyLoanSchemeSelect').value);
    const scheme = window.loadedSchemes?.find(s => s.id === sId);
    if (scheme) {
        document.getElementById('applyLoanAmount').value = scheme.defaultAmount;
        document.getElementById('applyLoanTenure').value = scheme.defaultTenureMonths;
        document.getElementById('applyLoanSchemePreview').innerText = 
            `RATE: ${scheme.interestRatePerAnnum}% P.A. | METHOD: ${scheme.interestCalculationMethod == 1 ? 'FLAT INTEREST' : 'REDUCING BALANCE'} | REPAYMENT: ${scheme.repaymentFrequency == 2 ? 'WEEKLY' : 'MONTHLY'}`;
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
            Swal.fire('LOAN APPLICATION SUBMITTED', `LOAN NUMBER: ${res.data.loanAccountNumber}`, 'success');
            switchTab('loans');
            loadDashboard();
        } else {
            Swal.fire('Error', res?.message || 'Loan application failed', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
}

async function directApproveLoan(id, loanNo, amt) {
    const confirm = await Swal.fire({
        title: 'APPROVE LOAN APPLICATION',
        text: `Approve loan ${loanNo} for ₹${amt.toLocaleString()}?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'YES, APPROVE'
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await api(`/api/loans/${id}/approve`, 'POST', {
            loanId: parseInt(id),
            approvedAmount: amt,
            remarks: 'Approved by Committee'
        });

        if (res && res.success) {
            Swal.fire('APPROVED', `Loan ${loanNo} is approved and ready for disbursement.`, 'success');
            loadLoans();
        }
    } catch {
        Swal.fire('Error', 'Approval failed', 'error');
    }
}

async function directDisburseLoan(id, loanNo, amt) {
    const confirm = await Swal.fire({
        title: 'DISBURSE LOAN',
        text: `Disburse ₹${amt.toLocaleString()} for loan ${loanNo}? This will generate full EMI schedule & post double-entry voucher!`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'YES, DISBURSE NOW'
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await api(`/api/loans/${id}/disburse`, 'POST', {
            loanId: parseInt(id),
            disbursedAmount: amt,
            disbursementMode: 1, // Cash
            firstEmiDate: new Date()
        });

        if (res && res.success) {
            Swal.fire('DISBURSED & ACTIVE', `Loan ${loanNo} is disbursed! EMI schedule generated.`, 'success');
            loadLoans();
            loadDashboard();
        }
    } catch {
        Swal.fire('Error', 'Disbursement failed', 'error');
    }
}

// Show Full Page Loan Passbook
async function viewLoanSchedule(id, loanNo) {
    try {
        const res = await api(`/api/loans/${id}/schedule`);
        if (res && res.success) {
            document.getElementById('scheduleLoanSubtitle').innerText = `LOAN ACCOUNT #${loanNo}`;
            const tbody = document.getElementById('scheduleTableBody');
            tbody.innerHTML = res.data.map(e => `
                <tr class="hover:bg-slate-50 transition">
                    <td class="py-3 px-4 font-mono font-black text-slate-500">${e.installmentNumber}</td>
                    <td class="py-3 px-4 font-bold text-slate-800">${new Date(e.dueDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-3 px-4 text-right font-mono">₹${Number(e.principalAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono">₹${Number(e.interestAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-slate-900">₹${Number(e.totalEmiAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono text-emerald-700 font-black">₹${Number(e.totalPaidAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-slate-900">₹${Number(e.totalOutstanding).toFixed(2)}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase ${e.statusName === 'Paid' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : (e.statusName === 'Overdue' ? 'bg-rose-50 text-rose-800 border border-rose-200' : 'bg-slate-100 text-slate-700')}">
                            ${e.statusName}
                        </span>
                    </td>
                </tr>
            `).join('');

            switchTab('loan-schedule');
        }
    } catch {
        Swal.fire('Error', 'Failed to load passbook', 'error');
    }
}

// 9. FIELD COLLECTION SHEET
async function setupCollectionSheet() {
    try {
        const query = selectedBranchId ? `?branchId=${selectedBranchId}` : '';
        const res = await api(`/api/centers${query}`);
        const selector = document.getElementById('sheetCenterSelector');
        if (res && res.success && selector) {
            selector.innerHTML = '<option value="">SELECT OPERATIONAL CENTER...</option>' + res.data.map(c => `<option value="${c.id}">${c.centerName.toUpperCase()} (${c.meetingDayName.toUpperCase()})</option>`).join('');
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
                tbody.innerHTML = '<tr><td colspan="10" class="text-center py-8 text-slate-400 font-black uppercase">No active loans due for this center on this date.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.map((item) => `
                <tr class="hover:bg-emerald-50/40 transition" data-loan-id="${item.loanId}">
                    <td class="py-3 px-4 font-mono font-black text-emerald-800">${item.loanAccountNumber}</td>
                    <td class="py-3 px-4 font-black text-slate-900 uppercase">${item.customerName}</td>
                    <td class="py-3 px-4 text-slate-500 uppercase font-bold">${item.groupName}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold">₹${Number(item.dueAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono text-rose-600 font-black">₹${Number(item.overdueAmount).toFixed(2)}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-slate-900 text-sm">₹${Number(item.totalReceivable).toFixed(2)}</td>
                    <td class="py-3 px-4 text-center">
                        <input type="number" class="sheet-collect-amt w-28 px-3 py-1.5 text-right font-black bg-slate-50 border-2 border-slate-200 rounded-xl text-xs focus:bg-white focus:border-emerald-600" value="${item.collectedAmount}" />
                    </td>
                    <td class="py-3 px-4 text-center">
                        <input type="number" class="sheet-savings-amt w-20 px-3 py-1.5 text-right font-bold bg-slate-50 border-2 border-slate-200 rounded-xl text-xs" value="${item.savingsDeposit}" />
                    </td>
                    <td class="py-3 px-4 text-center">
                        <select class="sheet-pay-mode text-[11px] font-black uppercase bg-slate-50 border-2 border-slate-200 rounded-xl px-2 py-1.5">
                            <option value="1">CASH</option>
                            <option value="4">UPI / QR</option>
                        </select>
                    </td>
                    <td class="py-3 px-4 text-right">
                        <button onclick="collectSingleFromSheet(${item.loanId}, this)" class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-black text-xs uppercase shadow-sm">
                            COLLECT
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
            row.classList.add('bg-emerald-100/60');
            btn.disabled = true;
            btn.innerText = 'PAID ✓';
            btn.className = 'px-3 py-1.5 bg-emerald-800 text-white rounded-xl font-black text-xs uppercase';

            // Show Dedicated Full Page Receipt
            showReceiptPage(res.data);
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
        Swal.fire('SELECT CENTER', 'Please choose an operational center first.', 'info');
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
        Swal.fire('NO PAYMENTS', 'No collections to submit.', 'info');
        return;
    }

    const confirm = await Swal.fire({
        title: 'CONFIRM BATCH COLLECTION',
        text: `Submit ${items.length} payments for this center?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'YES, SUBMIT BATCH'
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await api('/api/collection/batch-submit', 'POST', {
            centerId: parseInt(centerId),
            branchId: selectedBranchId ? parseInt(selectedBranchId) : (currentUser?.branchId || 1),
            items: items
        });

        if (res && res.success) {
            Swal.fire('SUCCESS', `${res.data} COLLECTIONS RECORDED & VOUCHERS POSTED!`, 'success');
            loadCollectionSheet();
            loadDashboard();
        }
    } catch {
        Swal.fire('Error', 'Batch submit failed.', 'error');
    }
}

// Show Dedicated Full Page Receipt
function showReceiptPage(col) {
    document.getElementById('rcpNumber').innerText = col.receiptNumber;
    document.getElementById('rcpDate').innerText = new Date(col.collectionDate).toLocaleString('en-GB');
    document.getElementById('rcpCustomer').innerText = `MEMBER ID: ${col.customerId}`;
    document.getElementById('rcpLoanNo').innerText = `LOAN ID: ${col.loanApplicationId}`;
    document.getElementById('rcpAmount').innerText = `₹${Number(col.totalAmountPaid).toLocaleString('en-IN', {minimumFractionDigits: 2})}`;
    document.getElementById('rcpPrincipal').innerText = `₹${Number(col.principalPortion).toFixed(2)}`;
    document.getElementById('rcpInterest').innerText = `₹${Number(col.interestPortion).toFixed(2)}`;
    switchTab('receipt');
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
                tbody.innerHTML = '<tr><td colspan="8" class="text-center py-8 text-slate-400 font-black uppercase">No savings accounts found.</td></tr>';
                return;
            }

            tbody.innerHTML = res.data.map(s => `
                <tr class="hover:bg-emerald-50/40 transition">
                    <td class="py-3 px-4 font-mono font-black text-emerald-800">${s.accountNumber}</td>
                    <td class="py-3 px-4 font-black text-slate-900 uppercase">${s.customerName}</td>
                    <td class="py-3 px-4 uppercase font-bold text-slate-700">${s.savingsSchemeName}</td>
                    <td class="py-3 px-4 text-right font-black text-slate-900 font-mono text-sm">₹${Number(s.currentBalance).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold text-slate-600">₹${Number(s.totalDeposited).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-right font-mono font-bold text-slate-600">₹${Number(s.totalWithdrawn).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center">
                        <span class="px-3 py-1 rounded-full text-[10px] font-black uppercase bg-emerald-50 text-emerald-800 border border-emerald-200">ACTIVE</span>
                    </td>
                    <td class="py-3 px-4 text-right text-slate-400 font-mono font-bold">${new Date(s.openedDate).toLocaleDateString('en-GB')}</td>
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
                    <td class="py-3 px-4 font-mono font-black text-slate-900">${a.accountCode}</td>
                    <td class="py-3 px-4 font-bold text-slate-800 uppercase">${a.accountName}</td>
                    <td class="py-3 px-4">
                        <span class="px-2.5 py-1 rounded-lg text-[10px] font-black uppercase ${getAccountClassBadge(a.classificationName)}">
                            ${a.classificationName}
                        </span>
                    </td>
                    <td class="py-3 px-4 text-right font-black text-slate-900 font-mono text-sm">
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
        case 'Asset': return 'bg-blue-50 text-blue-800 border border-blue-200';
        case 'Liability': return 'bg-amber-50 text-amber-800 border border-amber-200';
        case 'Equity': return 'bg-purple-50 text-purple-800 border border-purple-200';
        case 'Revenue': return 'bg-emerald-50 text-emerald-800 border border-emerald-200';
        case 'Expense': return 'bg-rose-50 text-rose-800 border border-rose-200';
        default: return 'bg-slate-100 text-slate-700';
    }
}

function switchAccountingTab(tab) {
    document.querySelectorAll('.acc-view').forEach(v => v.classList.add('hidden'));
    const target = document.getElementById(`acc-view-${tab}`);
    if (target) target.classList.remove('hidden');

    document.querySelectorAll('.acc-sub-btn').forEach(b => {
        b.classList.remove('bg-emerald-600', 'text-white');
        b.classList.add('text-slate-600');
    });
    const btn = document.getElementById(`acc-tab-${tab}`);
    if (btn) {
        btn.classList.add('bg-emerald-600', 'text-white');
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
                    <h4 class="font-black text-sm text-slate-900 uppercase tracking-wider">GENERAL LEDGER TRIAL BALANCE</h4>
                    <span class="text-xs px-3 py-1 rounded-full font-black uppercase ${d.isBalanced ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : 'bg-rose-50 text-rose-800 border border-rose-200'}">
                        ${d.isBalanced ? 'BALANCED (DEBIT = CREDIT) ✓' : 'OUT OF BALANCE'}
                    </span>
                </div>
                <table class="w-full text-left text-xs border-collapse">
                    <thead>
                        <tr class="bg-slate-50 border-b-2 border-slate-200 text-slate-600 uppercase font-black">
                            <th class="py-3 px-3">ACCOUNT CODE & NAME</th>
                            <th class="py-3 px-3">CLASSIFICATION</th>
                            <th class="py-3 px-3 text-right">DEBIT BALANCE (₹)</th>
                            <th class="py-3 px-3 text-right">CREDIT BALANCE (₹)</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100 font-bold">
                        ${d.rows.map(r => `
                            <tr>
                                <td class="py-3 px-3"><span class="font-mono font-black mr-2 text-emerald-700">${r.accountCode}</span>${r.accountName.toUpperCase()}</td>
                                <td class="py-3 px-3 text-slate-500 uppercase">${r.classification}</td>
                                <td class="py-3 px-3 text-right font-mono font-black">${r.debitBalance > 0 ? '₹' + Number(r.debitBalance).toLocaleString('en-IN', {minimumFractionDigits: 2}) : '--'}</td>
                                <td class="py-3 px-3 text-right font-mono font-black">${r.creditBalance > 0 ? '₹' + Number(r.creditBalance).toLocaleString('en-IN', {minimumFractionDigits: 2}) : '--'}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                    <tfoot>
                        <tr class="bg-emerald-50/50 font-black border-t-2 border-slate-300">
                            <td colspan="2" class="py-4 px-3 uppercase text-right text-slate-800">TOTAL:</td>
                            <td class="py-4 px-3 text-right text-emerald-800 font-mono text-sm">₹${Number(d.totalDebit).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                            <td class="py-4 px-3 text-right text-emerald-800 font-mono text-sm">₹${Number(d.totalCredit).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
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
                    <h4 class="font-black text-sm text-slate-900 uppercase tracking-wider">PROFIT & LOSS STATEMENT</h4>
                    <span class="text-sm font-black px-4 py-1.5 rounded-2xl uppercase ${d.netProfit >= 0 ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : 'bg-rose-50 text-rose-800 border border-rose-200'}">
                        NET PROFIT: ₹${Number(d.netProfit).toLocaleString('en-IN', {minimumFractionDigits: 2})}
                    </span>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-xs font-bold">
                    <div class="bg-emerald-50/50 p-5 rounded-3xl border-2 border-emerald-100 space-y-2.5">
                        <h5 class="font-black text-emerald-900 uppercase tracking-wider text-xs">OPERATING REVENUES</h5>
                        ${d.revenues.map(r => `
                            <div class="flex justify-between py-1.5 border-b border-emerald-100 uppercase">
                                <span>${r.accountName}</span>
                                <span class="font-black font-mono">₹${Number(r.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-3 font-black text-emerald-900 text-sm border-t-2 border-emerald-200">
                            <span>TOTAL REVENUE:</span>
                            <span class="font-mono">₹${Number(d.totalRevenue).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>

                    <div class="bg-rose-50/50 p-5 rounded-3xl border-2 border-rose-100 space-y-2.5">
                        <h5 class="font-black text-rose-900 uppercase tracking-wider text-xs">OPERATING EXPENSES</h5>
                        ${d.expenses.map(e => `
                            <div class="flex justify-between py-1.5 border-b border-rose-100 uppercase">
                                <span>${e.accountName}</span>
                                <span class="font-black font-mono">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-3 font-black text-rose-900 text-sm border-t-2 border-rose-200">
                            <span>TOTAL EXPENSES:</span>
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
                <h4 class="font-black text-sm text-slate-900 uppercase tracking-wider mb-4">BALANCE SHEET STATEMENT</h4>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-xs font-bold">
                    <div class="bg-blue-50/50 p-5 rounded-3xl border-2 border-blue-100 space-y-2.5">
                        <h5 class="font-black text-blue-900 uppercase tracking-wider text-xs">ASSETS</h5>
                        ${d.assets.map(a => `
                            <div class="flex justify-between py-1.5 border-b border-blue-100 uppercase">
                                <span>${a.accountName}</span>
                                <span class="font-black font-mono">₹${Number(a.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-3 font-black text-blue-900 text-sm border-t-2 border-blue-200">
                            <span>TOTAL ASSETS:</span>
                            <span class="font-mono">₹${Number(d.totalAssets).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                        </div>
                    </div>

                    <div class="bg-purple-50/50 p-5 rounded-3xl border-2 border-purple-100 space-y-2.5">
                        <h5 class="font-black text-purple-900 uppercase tracking-wider text-xs">LIABILITIES & EQUITY</h5>
                        ${d.liabilities.map(l => `
                            <div class="flex justify-between py-1.5 border-b border-purple-100 uppercase">
                                <span>${l.accountName}</span>
                                <span class="font-black font-mono">₹${Number(l.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        ${d.equities.map(e => `
                            <div class="flex justify-between py-1.5 border-b border-purple-100 uppercase">
                                <span>${e.accountName}</span>
                                <span class="font-black font-mono">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</span>
                            </div>
                        `).join('')}
                        <div class="flex justify-between pt-3 font-black text-purple-900 text-sm border-t-2 border-purple-200">
                            <span>TOTAL LIABILITIES & EQUITY:</span>
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
                    <td class="py-3 px-4 font-mono font-black text-emerald-800">${v.voucherNumber}</td>
                    <td class="py-3 px-4 font-mono font-bold">${new Date(v.voucherDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-3 px-4"><span class="px-2 py-0.5 rounded text-[10px] font-black uppercase bg-slate-100 text-slate-800">${v.voucherType}</span></td>
                    <td class="py-3 px-4 text-slate-600 uppercase font-bold">${v.narration}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-slate-900">₹${Number(v.totalAmount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-center text-slate-400 font-bold uppercase">${v.preparedBy || 'System'}</td>
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
                    <td class="py-3 px-4 font-mono font-black text-slate-700">${e.expenseNumber}</td>
                    <td class="py-3 px-4 font-mono font-bold">${new Date(e.expenseDate).toLocaleDateString('en-GB')}</td>
                    <td class="py-3 px-4 font-black text-slate-800 uppercase">${e.expenseCategoryName}</td>
                    <td class="py-3 px-4 font-bold text-slate-700 uppercase">${e.paidTo}</td>
                    <td class="py-3 px-4 font-bold uppercase">${e.paymentMode == 1 ? 'Cash' : 'Bank'}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-rose-600">₹${Number(e.amount).toLocaleString('en-IN', {minimumFractionDigits: 2})}</td>
                    <td class="py-3 px-4 text-slate-500 uppercase font-bold">${e.description || '--'}</td>
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
                <tr class="hover:bg-emerald-50/40 transition">
                    <td class="py-3 px-4 font-mono font-black text-emerald-800">${e.employeeCode}</td>
                    <td class="py-3 px-4 font-black text-slate-900 uppercase">${e.fullName}</td>
                    <td class="py-3 px-4">
                        <span class="font-black text-slate-800 uppercase">${e.designation?.title || 'Officer'}</span>
                        <span class="block text-[10px] text-slate-400 font-mono font-bold uppercase">${e.role}</span>
                    </td>
                    <td class="py-3 px-4 font-bold uppercase">${e.branch?.branchName || '--'}</td>
                    <td class="py-3 px-4 text-slate-600 font-mono font-bold">${e.phone} | ${e.email}</td>
                    <td class="py-3 px-4 text-right font-mono font-black text-slate-900">₹${Number(e.basicSalary).toLocaleString('en-IN')}</td>
                </tr>
            `).join('');
        }
    } catch (err) {
        console.error('Error loading employees', err);
    }
}

// 14. RESET / WIPE DUMMY DATA TO CLEAN DATABASE
async function confirmClearDummyData() {
    const confirm = await Swal.fire({
        title: 'WIPE DUMMY RECORDS?',
        text: 'This will remove all demo borrowers, sample loans, and collections, leaving your database completely clean for fresh production data!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#e11d48',
        confirmButtonText: 'YES, WIPE ALL DUMMY DATA'
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await api('/api/dashboard/reset-dummy-data', 'POST');
        if (res && res.success) {
            Swal.fire('DATABASE CLEANED!', res.message, 'success');
            loadDashboard();
            if (activeTab === 'customers') loadCustomers();
            if (activeTab === 'loans') loadLoans();
        } else {
            Swal.fire('Error', res?.message || 'Failed to clear data', 'error');
        }
    } catch {
        Swal.fire('Error', 'Connection failure', 'error');
    }
}

function quickApplyForCustomer(cid, name) {
    showApplyLoanPage().then(() => {
        setTimeout(() => {
            const sel = document.getElementById('applyLoanCustomerSelect');
            if (sel) sel.value = cid;
        }, 150);
    });
}

function exportLoanPortfolioCsv() {
    window.location.href = '/api/loans?pageSize=1000';
    Swal.fire('EXPORTING', 'Preparing CSV portfolio report...', 'info');
}

// 15. COMPANY PROFILE & SETTINGS
async function loadCompanyInfo() {
    try {
        const res = await api('/api/company');
        if (res && res.success && res.data) {
            const comp = res.data;
            const displayName = (comp.name || 'RAYHAN MICROFINANCE').toUpperCase();
            
            const topbarName = document.getElementById('topbarCompanyName');
            if (topbarName) topbarName.innerText = displayName;
            
            const dashTitle = document.getElementById('dashboardCompanyTitle');
            if (dashTitle) dashTitle.innerText = displayName;
        }
    } catch (err) {
        console.error('Error loading company info', err);
    }
}

async function openCompanyModal() {
    try {
        const res = await api('/api/company');
        if (res && res.success && res.data) {
            const comp = res.data;
            const setVal = (id, val) => {
                const el = document.getElementById(id);
                if (el) el.value = val || '';
            };
            setVal('compName', comp.name);
            setVal('compCode', comp.code);
            setVal('compRegNo', comp.registrationNumber);
            setVal('compTaxNo', comp.taxNumber);
            setVal('compPhone', comp.phone);
            setVal('compEmail', comp.email);
            setVal('compAddress', comp.address);
            setVal('compCity', comp.city);
            setVal('compState', comp.state);
            setVal('compPincode', comp.pincode);
            setVal('compCurrencySymbol', comp.currencySymbol || '₹');
            setVal('compCurrencyCode', comp.currencyCode || 'INR');
        }
    } catch (err) {
        console.error('Error opening company modal', err);
    }
    const modal = document.getElementById('companyModal');
    if (modal) modal.classList.remove('hidden');
}

function closeCompanyModal() {
    const modal = document.getElementById('companyModal');
    if (modal) modal.classList.add('hidden');
}

async function saveCompanyProfile() {
    const getVal = (id) => {
        const el = document.getElementById(id);
        return el ? el.value.trim() : '';
    };

    const payload = {
        name: getVal('compName'),
        code: getVal('compCode'),
        registrationNumber: getVal('compRegNo'),
        taxNumber: getVal('compTaxNo'),
        phone: getVal('compPhone'),
        email: getVal('compEmail'),
        address: getVal('compAddress'),
        city: getVal('compCity'),
        state: getVal('compState'),
        pincode: getVal('compPincode'),
        currencySymbol: getVal('compCurrencySymbol') || '₹',
        currencyCode: getVal('compCurrencyCode') || 'INR',
        country: 'India'
    };

    if (!payload.name || !payload.code) {
        Swal.fire('Required Fields', 'Please enter Company Legal Name and Short Code.', 'warning');
        return;
    }

    try {
        const res = await api('/api/company', 'PUT', payload);
        if (res && res.success) {
            Swal.fire({
                icon: 'success',
                title: 'COMPANY PROFILE SAVED!',
                text: 'Company profile updated successfully.',
                timer: 2000,
                showConfirmButton: false
            });
            closeCompanyModal();
            await loadCompanyInfo();
        } else {
            Swal.fire('Error', res?.message || 'Failed to save company profile', 'error');
        }
    } catch (err) {
        Swal.fire('Error', 'Unable to connect to server.', 'error');
    }
}
