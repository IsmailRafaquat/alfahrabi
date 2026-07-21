import { Component } from '@angular/core';

interface Student {
  name: string;
  percentage: number;
  rank: number;
  achievement: string;
  image?: string;
}

interface Course {
  title: string;
  level: string;
  description: string;
  price: number;
  rating: number;
  students: number;
  image?: string;
}

interface Teacher {
  name: string;
  position: string;
  qualification: string;
  image?: string;
  social?: {
    facebook?: string;
    twitter?: string;
    linkedin?: string;
  };
}

@Component({
  selector: 'app-alfarabi',
  standalone: false,
  templateUrl: './alfarabi.component.html',
  styleUrl: './alfarabi.component.scss'
})
export class AlfarabiComponent {
  // Grade 9 Top Students
  grade9Students: Student[] = [
    {
      name: 'Muhammad Ahmed',
      percentage: 95.5,
      rank: 1,
      achievement: 'Excellence in Mathematics & Science',
      image: 'assets/images/student1.jpg'
    },
    {
      name: 'Fatima Khan',
      percentage: 93.2,
      rank: 2,
      achievement: 'Outstanding in Computer Science',
      image: 'assets/images/student2.jpg'
    },
    {
      name: 'Ali Hassan',
      percentage: 91.8,
      rank: 3,
      achievement: 'Top in English & Urdu',
      image: 'assets/images/student3.jpg'
    }
  ];

  // Grade 10 Top Students
  grade10Students: Student[] = [
    {
      name: 'Ayesha Malik',
      percentage: 96.8,
      rank: 1,
      achievement: 'Perfect Score in Biology',
      image: 'assets/images/student4.jpg'
    },
    {
      name: 'Hamza Afridi',
      percentage: 94.5,
      rank: 2,
      achievement: 'Excellence in Physics & Chemistry',
      image: 'assets/images/student5.jpg'
    },
    {
      name: 'Zainab Shah',
      percentage: 92.9,
      rank: 3,
      achievement: 'Top in Pakistan Studies',
      image: 'assets/images/student6.jpg'
    }
  ];

  // Computer Courses
  courses: Course[] = [
    {
      title: 'Microsoft Office Suite',
      level: 'Beginner',
      description: 'Master Word, Excel, PowerPoint, and more',
      price: 3000,
      rating: 5,
      students: 45,
      image: 'assets/images/course1.jpg'
    },
    {
      title: 'Web Development Basics',
      level: 'Intermediate',
      description: 'HTML, CSS, and JavaScript fundamentals',
      price: 5000,
      rating: 5,
      students: 38,
      image: 'assets/images/course2.jpg'
    },
    {
      title: 'Graphic Design & Video Editing',
      level: 'Advanced',
      description: 'Adobe Photoshop, Illustrator & Premiere Pro',
      price: 6500,
      rating: 5,
      students: 32,
      image: 'assets/images/course3.jpg'
    },
    {
      title: 'Python Programming',
      level: 'Advanced',
      description: 'Learn coding from scratch to advanced',
      price: 7000,
      rating: 5,
      students: 28,
      image: 'assets/images/course4.jpg'
    }
  ];

  // Teachers
  teachers: Teacher[] = [
    {
      name: 'Mr. Khalid Rahman',
      position: 'Principal & Mathematics Teacher',
      qualification: 'M.Sc Mathematics, B.Ed',
      image: 'assets/images/teacher1.jpg',
      social: {
        facebook: '#',
        twitter: '#',
        linkedin: '#'
      }
    },
    {
      name: 'Ms. Nadia Iqbal',
      position: 'Computer Science Teacher',
      qualification: 'BS Computer Science, MCS',
      image: 'assets/images/teacher2.jpg',
      social: {
        facebook: '#',
        twitter: '#',
        linkedin: '#'
      }
    },
    {
      name: 'Mr. Asad Khan',
      position: 'Science Teacher',
      qualification: 'M.Sc Physics, B.Ed',
      image: 'assets/images/teacher3.jpg',
      social: {
        facebook: '#',
        twitter: '#',
        linkedin: '#'
      }
    },
    {
      name: 'Ms. Sara Ahmed',
      position: 'English Teacher',
      qualification: 'MA English, B.Ed',
      image: 'assets/images/teacher4.jpg',
      social: {
        facebook: '#',
        twitter: '#',
        linkedin: '#'
      }
    }
  ];

  // School Statistics
  stats = {
    computerLabCapacity: 85,
    qualifiedTeachers: 92,
    studentSuccessRate: 88
  };

  // Popular Subjects
  subjects = [
    { name: 'Mathematics', icon: '📊' },
    { name: 'Science', icon: '🔬' },
    { name: 'Computer', icon: '💻' },
    { name: 'English', icon: '📖' },
    { name: 'Pakistan Studies', icon: '🌍' },
    { name: 'Islamiyat', icon: '☪️' }
  ];

  // FAQ Data
  faqs = [
  {
    question: 'What are the admission requirements?',
    answer:
      'Students need to submit their previous academic records, birth certificate, and complete the admission form. An entrance test may be required for certain grades.',
    open: true
  },
  {
    question: 'What computer courses are available?',
    answer:
      'Microsoft Office, Web Development (HTML/CSS/JS), Graphic Design & Video Editing, and Python Programming courses are available.',
    open: false
  },
  {
    question: 'What are the school timings?',
    answer:
      'Monday to Saturday: 8:00 AM to 1:30 PM. Office hours: 8:00 AM to 2:00 PM.',
    open: false
  },
  {
    question: 'Do you offer transportation services?',
    answer:
      'Yes, transportation is available on selected routes. Please contact the admin office for route availability and charges.',
    open: false
  },
  {
    question: 'What is the fee structure?',
    answer:
      'Fee depends on grade and selected facilities. Please contact us for the latest fee structure.',
    open: false
  },
  {
    question: 'Are there scholarship opportunities?',
    answer:
      'Yes, scholarships are offered for high achievers and deserving students based on criteria.',
    open: false
  },
  {
    question: 'What extracurricular activities are offered?',
    answer:
      'Sports, debates, quizzes, science exhibitions, and computer clubs are offered throughout the year.',
    open: false
  },
  {
    question: 'How can parents track student progress?',
    answer:
      'Parents can track progress through monthly tests, report cards, and parent-teacher meetings.',
    open: false
  }
];

  constructor() {}

  // Method to get star rating display
  getStars(rating: number): string {
    return '⭐'.repeat(rating);
  }

  // Method to format currency
  formatPrice(price: number): string {
    return `PKR ${price.toLocaleString()}`;
  }

  // Method to toggle FAQ
  toggleFaq(index: number): void {
    this.faqs[index].open = !this.faqs[index].open;
  }

  // Method to handle form submission
  onSubmitContact(event: Event): void {
    event.preventDefault();
    // Handle contact form submission
    console.log('Contact form submitted');
  }

  // Method to handle email signup
  onEmailSignup(event: Event): void {
    event.preventDefault();
    // Handle email signup
    console.log('Email signup submitted');
  }
}