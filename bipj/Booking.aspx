<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="Booking.aspx.cs" Inherits="bipj.Booking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">

    <!-- jQuery & jQuery UI -->
    <link
      rel="stylesheet"
      href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css"
    />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>




  <style>
    /* === WIZARD HEADER === */
    .wizard { text-align: center; margin: 40px 0; }
    .wizard .step {
      display: inline-block;
      width: 40px; height: 40px;
      line-height: 40px;
      border-radius: 50%;
      background: #eee;
      color: #777;
      margin: 0 10px;
      font-weight: bold;
      position: relative;
    }
    .wizard .step.active { background: #433e8e; color: #fff; }
    .wizard .step.completed:after,
    .wizard .step.active:after {
      background: #433e8e;
    }
    .wizard .step:after {
      content: '';
      position: absolute;
      top: 50%; right: -30px;
      width: 60px; height: 4px;
      background: #eee;
      transform: translateY(-50%);
      z-index: -1;
    }
    .wizard .step:last-child:after { display: none; }

    /* === PANELS === */
    .step-panel { display: none; background: #f5f3ff; border-radius: 12px; padding: 32px; margin: 0 auto 40px; max-width: 800px; }
    .step-panel.active { display: block; }

    .btn-next, .btn-prev, .btn-confirm {
      background: #433e8e; color: #fff; border: none; border-radius: 6px;
      padding: 10px 24px; cursor: pointer; font-size: 1rem; margin-top: 20px;
    }
    .btn-prev { background: #aaa; margin-right: 10px; }

    /* === STEP 1: SESSION TYPE === */
    .card-grid { display: flex; gap: 24px; justify-content: center; flex-wrap: wrap; }
    .card-grid .card {
      background: #fff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      flex: 1 1 240px; max-width: 280px; padding: 20px; text-align: center;
      cursor: pointer; transition: transform .2s;
    }
    .card-grid .card.selected { border: 2px solid #433e8e; }
    .card-grid .card:hover { transform: translateY(-4px); }
    .card-grid .card i { font-size: 2.5rem; margin-bottom: 12px; color: #433e8e; }
    .card-grid .card h4 { margin: 8px 0; }
    .card-grid .card p { color: #555; font-size: .9rem; }

    /* === STEP 2: ADVISOR LIST === */
    .advisor-search { display: flex; gap: 12px; margin-bottom: 20px; }
    .advisor-search input { flex:1; padding:8px 12px; border-radius:6px; border:1px solid #ccc; }
    .advisor-search button { padding:8px 12px; border:none; background:#433e8e; color:#fff; border-radius:6px; cursor:pointer; }
    .advisor-list .advisor-card {
      display:flex; gap:16px; padding:16px; background:#fff; border-radius:12px; margin-bottom:16px; border:1px solid #ddd;
    }
    .advisor-list .advisor-card.selected { border-color:#433e8e; }
    .advisor-list img { width:64px; height:64px; border-radius:50%; object-fit:cover; }
    .advisor-info h5 { margin:0 0 4px; }
    .advisor-info .stars { color:#f5a623; margin-bottom:8px; }
    .advisor-info p { font-size:.9rem; color:#555; margin:4px 0; }
    .advisor-info .specialties { font-size:.85rem; color:#333; }
    .advisor-info .select-btn { margin-top: 8px; }

    /* === STEP 3: DATE & TIME === */
    .calendar-times { display:flex; gap:32px; flex-wrap:wrap; justify-content:center; }
    .timeslots { display:grid; grid-template-columns:repeat(auto-fill, minmax(80px,1fr)); gap:12px; }
    .timeslots button {
      padding:8px; border:none; border-radius:6px; background:#eee; cursor:pointer;
    }
    .timeslots button.selected { background:#433e8e; color:#fff; }
    .legend { display:flex; gap:16px; margin-top:12px; font-size:.85rem; }
    .legend span { display:flex; align-items:center; gap:4px; }
    .legend .avail { width:12px;height:12px;background:#eee;border-radius:2px; }
    .legend .sel   { width:12px;height:12px;background:#433e8e;border-radius:2px; }

    /* === STEP 4 & 5: FORM & CONFIRMATION === */
    .field-group { margin-bottom:16px; }
    .field-group label { display:block; margin-bottom:4px; font-weight:bold; }
    .field-group input, .field-group textarea {
      width:100%; padding:8px;border:1px solid #ccc;border-radius:6px;
    }
    .summary { background:#fff; padding:24px; border-radius:12px; }
    .summary dt { font-weight:bold; margin-top:12px; }
    .summary dd { margin:4px 0 12px; }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center; color:#3b3350; margin-top:24px;">Book a Session</h2>

  <!-- Wizard Header -->
  <div class="wizard">
    <div class="step active"    data-step="1">1</div>
    <div class="step"           data-step="2">2</div>
    <div class="step"           data-step="3">3</div>
    <div class="step"           data-step="4">4</div>
    <div class="step"           data-step="5">5</div>
  </div>

  <!-- STEP 1: Select Session Type -->
  <div id="step1" class="step-panel active">
    <h3>Choose Your Session Type</h3>
    <div class="card-grid">
      <div class="card" data-type="Individual">
        <i class="fas fa-user"></i>
        <h4>Individual</h4>
        <p>1-on-1 personal guidance tailored to your needs</p>
      </div>
      <div class="card" data-type="Group">
        <i class="fas fa-users"></i>
        <h4>Group</h4>
        <p>Shared learning and discussion. Max 10 participants</p>
      </div>
    </div>
    <button class="btn-next" onclick="nextStep()">Next</button>
  </div>

  <!-- STEP 2: Select Advisor -->
  <div id="step2" class="step-panel">
    <h3>Select Your Advisor</h3>
    <div class="advisor-search">
      <input type="text" id="advisorSearch" placeholder="Type a name or specialty…" oninput="filterAdvisors()" />
      <button onclick="filterAdvisors()"><i class="fas fa-search"></i></button>
    </div>
    <div class="advisor-list" id="advisorList">
      <asp:Repeater ID="rptAdvisors" runat="server">
        <ItemTemplate>
          <div class="advisor-card" data-id='<%# Eval("AdvisorId") %>' data-name='<%# Eval("Name") %>'>
            <img src='<%# Eval("PhotoPath") %>' alt='<%# Eval("Name") %>' />
            <div class="advisor-info">
              <h5><%# Eval("Category") %><br/><small><%# Eval("Name") %></small></h5>
              <div class="stars">Rating: <%# Eval("Rating") %>★</div>
              <p><%# Eval("Bio") %></p>
              <p class="specialties">
                <%# Eval("Specialty1") %><%# String.IsNullOrEmpty(Eval("Specialty2").ToString())? "" : ", " + Eval("Specialty2") %>
                <%# String.IsNullOrEmpty(Eval("Specialty3").ToString())? "" : ", " + Eval("Specialty3") %>
              </p>
              <button class="btn-next select-btn" onclick="selectAdvisor(this)">Select</button>
            </div>
          </div>
        </ItemTemplate>
      </asp:Repeater>
    </div>
    <button class="btn-prev" onclick="prevStep()">Back</button>
    <button class="btn-next" onclick="nextStep()" disabled id="toStep3">Next</button>
  </div>

  <!-- STEP 3: Pick Date & Time -->
<div id="step3" class="step-panel">
  <h3>Select Date &amp; Time</h3>

  <div class="calendar-times">
    <!-- 1) The datepicker will render here -->
    <div id="datepicker"></div>

    <!-- 2) When a date is selected, loadSlots() will fill this -->
    <div>
      <div class="timeslots" id="timeslots">
        <!-- timeslot buttons go here -->
      </div>
      <div class="legend">
        <span><i class="avail"></i> Available</span>
        <span><i class="sel"></i> Selected</span>
      </div>
    </div>
  </div>

  <button class="btn-prev" onclick="prevStep()">Back</button>
  <!-- Next is disabled until a time is chosen -->
  <button class="btn-next" onclick="nextStep()" disabled id="toStep4">Next</button>
</div>


  <!-- STEP 4: Additional Info -->
  <div id="step4" class="step-panel">
    <h3>Additional Information</h3>
    <div class="field-group">
      <label for="yourName">Name:</label>
      <input type="text" id="yourName" />
    </div>
    <div class="field-group">
      <label for="yourEmail">Email:</label>
      <input type="email" id="yourEmail" />
    </div>
    <div class="field-group">
      <label for="focus">What would you like to focus on?</label>
      <textarea id="focus" rows="3"></textarea>
    </div>
    <button class="btn-prev" onclick="prevStep()">Back</button>
    <button class="btn-next" onclick="nextStep()">Next</button>
  </div>

  <!-- STEP 5: Confirmation -->
  <div id="step5" class="step-panel">
    <h3>Confirm Your Booking</h3>
    <dl class="summary">
      <dt>Session Type:</dt><dd id="confType"></dd>
      <dt>Advisor:</dt><dd id="confAdvisor"></dd>
      <dt>Date &amp; Time:</dt><dd id="confDateTime"></dd>
      <dt>Name:</dt><dd id="confName"></dd>
      <dt>Email:</dt><dd id="confEmail"></dd>
      <dt>Focus:</dt><dd id="confFocus"></dd>
    </dl>
    <button class="btn-prev" onclick="prevStep()">Back</button>
    <button class="btn-confirm" onclick="confirmBooking()">Confirm Booking</button>
  </div>
</asp:Content>

<asp:Content ID="Scripts" ContentPlaceHolderID="scripts" runat="server">
  <script src="https://kit.fontawesome.com/a076d05399.js"></script>
  <script>
    let currentStep = 1;
      const booking = { type: null, advisorId: null, advisorName: null, date: null, time: null, name: '', email: '', focus: '' };

      $(function () {
          $("#datepicker").datepicker({
              dateFormat: "yy-mm-dd",
              minDate: 0,
              onSelect: function (dateText) {
                  booking.date = dateText;
                  loadSlots();
                  document.getElementById("toStep4").disabled = true;
              }
          });
      });

    function showStep(n) {
      document.querySelectorAll('.step-panel').forEach(p=>p.classList.remove('active'));
      document.getElementById('step'+n).classList.add('active');
      document.querySelectorAll('.wizard .step').forEach(s=>{
        const i=+s.dataset.step;
        s.classList.toggle('active', i===n);
        s.classList.toggle('completed', i<n);
      });
      currentStep=n;
    }
    function nextStep(){
      // validation per step
      if(currentStep===1 && !booking.type) { alert('Select a session type'); return; }
      if(currentStep===2 && !booking.advisorId) return;
      if(currentStep===3 && (!booking.date||!booking.time)) return;
      if(currentStep===4){
        booking.name  = document.getElementById('yourName').value.trim();
        booking.email = document.getElementById('yourEmail').value.trim();
        booking.focus = document.getElementById('focus').value.trim();
        if(!booking.name||!booking.email) { alert('Name and email required'); return; }
        populateConfirmation();
      }
      if(currentStep<5) showStep(currentStep+1);
    }
    function prevStep(){
      if(currentStep>1) showStep(currentStep-1);
    }

    // Step1
    document.querySelectorAll('#step1 .card').forEach(c=>{
      c.onclick=()=> {
        document.querySelectorAll('#step1 .card').forEach(x=>x.classList.remove('selected'));
        c.classList.add('selected');
        booking.type = c.dataset.type;
      };
    });

    // Step2
    function filterAdvisors(){
      const q = document.getElementById('advisorSearch').value.toLowerCase();
      document.querySelectorAll('.advisor-card').forEach(card=>{
        card.style.display = card.dataset.name.toLowerCase().includes(q)?'':'none';
      });
    }
    function selectAdvisor(btn){
      document.querySelectorAll('.advisor-card').forEach(x=>x.classList.remove('selected'));
      const card=btn.closest('.advisor-card');
      card.classList.add('selected');
      booking.advisorId   = card.dataset.id;
      booking.advisorName = card.dataset.name;
      document.getElementById('toStep3').disabled=false;
    }

    // Step3: load times after user picks date
    // You need to integrate your datepicker to set booking.date and then call loadSlots()
    function loadSlots(){
      fetch(`Booking.aspx/GetBookedSlots`, {
        method:'POST',
        headers:{'Content-Type':'application/json'},
        body: JSON.stringify({ advisorId:+booking.advisorId, date: booking.date })
      })
      .then(r=>r.json())
      .then(data=>{
        const taken = data.d; // array of "HH:mm"
        const container = document.getElementById('timeslots');
        container.innerHTML='';
        ['09:00','10:00','11:00','13:00','14:00','15:00'].forEach(t=>{
          const btn = document.createElement('button');
          btn.innerText = t;
          if(taken.includes(t)) btn.disabled=true;
          btn.onclick=()=>{
            document.querySelectorAll('#timeslots button').forEach(x=>x.classList.remove('selected'));
            btn.classList.add('selected');
            booking.time=t;
            document.getElementById('toStep4').disabled=false;
          };
          container.appendChild(btn);
        });
      });
    }

    // Step5
    function populateConfirmation(){
      document.getElementById('confType').innerText    = booking.type;
      document.getElementById('confAdvisor').innerText = booking.advisorName;
      document.getElementById('confDateTime').innerText= `${booking.date}, ${booking.time}`;
      document.getElementById('confName').innerText    = booking.name;
      document.getElementById('confEmail').innerText   = booking.email;
      document.getElementById('confFocus').innerText   = booking.focus;
    }
    function confirmBooking(){
      fetch('Booking.aspx/SaveBooking', {
        method:'POST',
        headers:{'Content-Type':'application/json'},
        body: JSON.stringify({
          userId: /* your logged-in user ID */,
          advisorId: +booking.advisorId,
          sessionType: booking.type,
          date: booking.date,
          time: booking.time,
          focus: booking.focus
        })
      })
      .then(r=>r.json())
      .then(res=>{
        if(res.d) {
          alert('Session booked successfully!');
          location.href='Home.aspx';
        } else alert('Booking failed. Try again.');
      });
    }

    // initialize
    showStep(1);
  </script>
</asp:Content>

