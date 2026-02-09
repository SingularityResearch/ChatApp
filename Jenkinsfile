pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        SOLUTION_FILE = 'ChatApp.sln' 
        PROJECT_FILE = 'ChatApp/ChatApp.csproj'
        BUILD_CONFIGURATION = 'Release'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                echo 'Restoring dependencies...'
                bat "dotnet restore ${PROJECT_FILE}"
            }
        }

        stage('Build') {
            steps {
                echo 'Building application...'
                bat "dotnet build ${PROJECT_FILE} -c ${BUILD_CONFIGURATION} --no-restore"
            }
        }

        stage('Test') {
            steps {
                echo 'Running tests...'
                // Placeholder: Add test command if/when unit tests are added
                // bat "dotnet test ${PROJECT_FILE} -c ${BUILD_CONFIGURATION} --no-build"
                echo 'No tests found to run.'
            }
        }

        stage('Publish') {
            steps {
                echo 'Publishing application...'
                bat "dotnet publish ${PROJECT_FILE} -c ${BUILD_CONFIGURATION} -o ./publish --no-build"
            }
        }
        
        stage('Deploy') {
             steps {
                 echo 'Deploying...'
                 // Example: Copy to IIS folder or Deployment Server
                 // bat "xcopy /s /y ./publish C:\inetpub\wwwroot\ChatApp"
                 echo 'Deployment step needs to be configured based on target environment.'
             }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'publish/**/*', allowEmptyArchive: true
        }
        success {
            echo 'Build and Deployment Succeeded.'
        }
        failure {
            echo 'Build or Deployment Failed.'
        }
    }
}
