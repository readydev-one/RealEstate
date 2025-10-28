#!/bin/bash
# infrastructure/scripts/setup-gcp.sh
# Google Cloud Platform Setup Script

set -e

# Configuration
PROJECT_ID="your-project-id"
REGION="us-central1"
SERVICE_NAME="realestate-api"
BUCKET_NAME="${PROJECT_ID}-documents"

echo "Setting up Google Cloud Platform for Real Estate API..."

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "Enabling required APIs..."
gcloud services enable \
    run.googleapis.com \
    firestore.googleapis.com \
    storage.googleapis.com \
    cloudscheduler.googleapis.com \
    gmail.googleapis.com \
    secretmanager.googleapis.com \
    cloudbuild.googleapis.com

# Create Firestore database
echo "Creating Firestore database..."
gcloud firestore databases create --location=$REGION || echo "Firestore database already exists"

# Create Cloud Storage bucket
echo "Creating Cloud Storage bucket..."
gcloud storage buckets create gs://$BUCKET_NAME \
    --location=$REGION \
    --uniform-bucket-level-access || echo "Bucket already exists"

# Create service account for the application
echo "Creating service account..."
SERVICE_ACCOUNT_NAME="realestate-api-sa"
SERVICE_ACCOUNT_EMAIL="${SERVICE_ACCOUNT_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

gcloud iam service-accounts create $SERVICE_ACCOUNT_NAME \
    --display-name="Real Estate API Service Account" || echo "Service account already exists"

# Grant necessary permissions
echo "Granting permissions..."
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${SERVICE_ACCOUNT_EMAIL}" \
    --role="roles/datastore.user"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${SERVICE_ACCOUNT_EMAIL}" \
    --role="roles/storage.objectAdmin"

gcloud storage buckets add-iam-policy-binding gs://$BUCKET_NAME \
    --member="serviceAccount:${SERVICE_ACCOUNT_EMAIL}" \
    --role="roles/storage.objectAdmin"

# Create service account key for Gmail API
echo "Creating service account key..."
gcloud iam service-accounts keys create gmail-service-account-key.json \
    --iam-account=$SERVICE_ACCOUNT_EMAIL || echo "Key already exists"

# Create secrets
echo "Creating secrets..."
echo "your-super-secret-jwt-key-at-least-32-characters-long" | \
    gcloud secrets create jwt-secret --data-file=- || echo "Secret already exists"

echo "your-32-byte-encryption-key-here!!" | \
    gcloud secrets create encryption-key --data-file=- || echo "Secret already exists"

cat gmail-service-account-key.json | \
    gcloud secrets create gmail-service-account --data-file=- || echo "Secret already exists"

# Grant service account access to secrets
echo "Granting secret access..."
for secret in jwt-secret encryption-key gmail-service-account; do
    gcloud secrets add-iam-policy-binding $secret \
        --member="serviceAccount:${SERVICE_ACCOUNT_EMAIL}" \
        --role="roles/secretmanager.secretAccessor"
done

echo "GCP setup complete!"
echo ""
echo "Next steps:"
echo "1. Update your appsettings.json with:"
echo "   - ProjectId: $PROJECT_ID"
echo "   - StorageBucket: $BUCKET_NAME"
echo "2. Set up GitHub secrets for CI/CD:"
echo "   - GCP_PROJECT_ID: $PROJECT_ID"
echo "   - GCP_SA_KEY: (contents of service account key JSON)"
echo "   - GCP_SA_EMAIL: $SERVICE_ACCOUNT_EMAIL"
echo