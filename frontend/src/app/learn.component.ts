import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'tmc-learn',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './learn.component.html',
  styleUrl: './learn.component.scss'
})
export class LearnComponent {
  // TODO: Break this into smaller lesson components when we start adding quizzes or interactive exercises.
}

