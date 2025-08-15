<%@ Page Title="Welcome" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.master"
    AutoEventWireup="true" CodeBehind="Landing.aspx.cs" Inherits="bipj.Landing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <style>
        /* Brand */
        :root {
            --brand: #3B387E;
            --brand-dark: #2d2a66;
        }

        /* Hero */
        .hero {
            background: radial-gradient(1200px 600px at 90% -10%, rgba(59,56,126,.25), transparent 60%),
                        linear-gradient(135deg, var(--brand) 0%, #5a56b1 60%, #7c78de 100%);
            color: #fff;
            border-radius: 1.25rem;
            padding: 3.5rem 2rem;
            position: relative;
            overflow: hidden;
        }
        .hero .badge {
            background: rgba(255,255,255,.15);
            border: 1px solid rgba(255,255,255,.25);
            color: #fff;
        }
        .hero-illustration {
            position: absolute; right: 2rem; bottom: -1rem; opacity: .12; font-size: 10rem; pointer-events: none;
        }

        /* Sections */
        .section-title { color: var(--brand); }
        .feature-card {
            border: 1px solid #eee; border-radius: 1rem;
            transition: transform .2s ease, box-shadow .2s ease;
        }
        .feature-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 24px rgba(0,0,0,.08);
        }

        /* CTA */
        .btn-brand { background: var(--brand); border-color: var(--brand); }
        .btn-brand:hover { background: var(--brand-dark); border-color: var(--brand-dark); }

        /* Stats */
        .stat {
            border-radius: 1rem;
            background: #f7f7ff;
            padding: 1.5rem;
        }

        /* Testimonials */
        .quote {
            border-left: 4px solid var(--brand);
            background: #fafafe;
            padding: 1rem 1.25rem;
            border-radius: .5rem;
        }
    </style>

    <!-- HERO -->
    <section class="container mt-4">
        <div class="hero">
            <span class="badge rounded-pill px-3 py-2">Financial Literacy • ASP.NET</span>
            <h1 class="display-5 fw-semibold mt-3">
                Learn money skills. <span class="fw-light">Build real confidence.</span>
            </h1>
            <p class="lead mt-2 mb-4">
                Bite-sized lessons, interactive modules, and progress tracking — designed for students and busy professionals.
            </p>

            <div class="d-flex gap-2 flex-wrap">
                <a href="Loginpage.aspx" class="btn btn-light btn-lg px-4">Log in</a>
                <a href="Signup.aspx" class="btn btn-brand btn-lg px-4 text-white">Get started free</a>
                <a href="#features" class="btn btn-outline-light btn-lg px-4">Explore features</a>
            </div>

            <div class="hero-illustration">₿ $ ¥ ₩ €</div>
        </div>
    </section>

    <!-- TRUST / STATS -->
    <section class="container mt-5">
        <div class="row g-3">
            <div class="col-6 col-md-3">
                <div class="stat text-center">
                    <div class="h3 mb-0">120+</div>
                    <small class="text-muted">Interactive pages</small>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="stat text-center">
                    <div class="h3 mb-0">5 min</div>
                    <small class="text-muted">Avg. lesson time</small>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="stat text-center">
                    <div class="h3 mb-0">Gamified</div>
                    <small class="text-muted">Quizzes & badges</small>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="stat text-center">
                    <div class="h3 mb-0">Accessible</div>
                    <small class="text-muted">Screen-reader friendly</small>
                </div>
            </div>
        </div>
    </section>

    <!-- FEATURES -->
    <section id="features" class="container mt-5">
        <h2 class="section-title fw-semibold mb-3">Why you’ll love it</h2>
        <div class="row g-4">
            <div class="col-md-4">
                <div class="feature-card p-4 h-100">
                    <h5 class="mb-2">Structured Modules</h5>
                    <p class="text-muted mb-3">From budgeting to investing — progress through clear, guided paths.</p>
                    <ul class="mb-0">
                        <li>Short, focused lessons</li>
                        <li>Real-world examples</li>
                        <li>Auto-save progress</li>
                    </ul>
                </div>
            </div>

            <div class="col-md-4">
                <div class="feature-card p-4 h-100">
                    <h5 class="mb-2">Smart Assistance</h5>
                    <p class="text-muted mb-3">Built-in AI assistant for Q&A and voice responses with on-screen captions.</p>
                    <ul class="mb-0">
                        <li>Context-aware answers</li>
                        <li>Voice + text output</li>
                        <li>Works on mobile</li>
                    </ul>
                </div>
            </div>

            <div class="col-md-4">
                <div class="feature-card p-4 h-100">
                    <h5 class="mb-2">Practice & Assess</h5>
                    <p class="text-muted mb-3">Quizzes, checkpoints, and certificates to validate your learning.</p>
                    <ul class="mb-0">
                        <li>Instant feedback</li>
                        <li>Downloadable certificate</li>
                        <li>Sharable progress</li>
                    </ul>
                </div>
            </div>
        </div>
    </section>

    <!-- TESTIMONIALS -->
    <section class="container mt-5">
        <h2 class="section-title fw-semibold mb-3">What learners say</h2>
        <div class="row g-4">
            <div class="col-md-6">
                <div class="quote">
                    “Clear and practical. I finally understand credit cards and budgeting.”
                    <div class="small text-muted mt-2">— Mei Lin, Student</div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="quote">
                    “Short lessons fit my schedule. The AI helper is surprisingly good.”
                    <div class="small text-muted mt-2">— Daniel, Working Professional</div>
                </div>
            </div>
        </div>
    </section>

    <!-- CALL TO ACTION -->
    <section class="container my-5">
        <div class="p-4 p-md-5 text-center border rounded-4"
             style="background: linear-gradient(135deg, #f7f7ff 0%, #ffffff 100%);">
            <h3 class="fw-semibold mb-2" style="color: var(--brand);">Ready to level up your money skills?</h3>
            <p class="text-muted mb-4">Join free, track progress, and earn badges as you learn.</p>
            <div class="d-flex justify-content-center gap-2">
                <a href="Signup.aspx" class="btn btn-brand px-4 text-white">Create an account</a>
                <a href="Loginpage.aspx" class="btn btn-outline-secondary px-4">I already have an account</a>
            </div>
        </div>
    </section>

    <!-- Smooth scroll for “Explore features” -->
    <script>
        document.addEventListener('click', function (e) {
            const a = e.target.closest('a[href^="#"]');
            if (!a) return;
            const el = document.querySelector(a.getAttribute('href'));
            if (el) {
                e.preventDefault();
                el.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    </script>

</asp:Content>
